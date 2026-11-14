import { useRef, useState } from 'react';
import { ActivityIndicator, ScrollView, Text, View } from 'react-native';
import { BarcodeScanningResult, CameraView, useCameraPermissions } from 'expo-camera';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/AuthContext';
import { useAppLock } from '../lock/AppLockContext';
import { ApiError } from '../communication/ApiClient';
import { AppButton } from '../components/AppButton';
import { ChallengeSecretView } from '../components/ChallengeSecretView';
import { StatusScreen } from '../components/StatusScreen';
import { parseChallengeQrCode } from '../helpers/ChallengeHelpers';
import { useThemeColors } from '../hooks/useThemeColors';
import { CommonStyles, Spacing } from '../styles/Theme';
import { Models } from '../types/models';

const ScanScreen = () => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const auth = useAuth();
    const appLock = useAppLock();
    const [ permission, requestPermission ] = useCameraPermissions();
    const [ isScanning, setIsScanning ] = useState<boolean>(true);
    const [ isLoading, setIsLoading ] = useState<boolean>(false);
    const [ challenge, setChallenge ] = useState<Models.MenschIdChallenge>();
    const [ errorMessage, setErrorMessage ] = useState<string>();
    const isHandlingScan = useRef<boolean>(false);

    const loadChallenge = async (challengeId: string) => {
        setIsLoading(true);
        try {
            const loadedChallenge = await auth.apiClient.get<Models.MenschIdChallenge>(`api/challenges/${encodeURIComponent(challengeId)}`);
            setChallenge(loadedChallenge);
        } catch (error) {
            const isNotFound = error instanceof ApiError && error.statusCode === 404;
            setErrorMessage(t(isNotFound ? 'Challenge_NotFound' : 'Challenge_CouldNotLoad'));
        } finally {
            setIsLoading(false);
        }
    };

    const onBarcodeScanned = (result: BarcodeScanningResult) => {
        if(isHandlingScan.current) {
            return;
        }
        const challengeId = parseChallengeQrCode(result.data);
        if(!challengeId) {
            return;
        }
        isHandlingScan.current = true;
        setIsScanning(false);
        loadChallenge(challengeId);
    };

    const scanAgain = () => {
        isHandlingScan.current = false;
        setChallenge(undefined);
        setErrorMessage(undefined);
        setIsScanning(true);
    };

    if(!permission) {
        return <StatusScreen isLoading message={t('Loading')} />;
    }
    if(!permission.granted) {
        return (
            <StatusScreen
                message={t('Scan_PermissionRequired')}
                actionTitle={t('Scan_GrantPermission')}
                onAction={() => appLock.runWithoutLock(requestPermission)}
            />
        );
    }
    if(isScanning) {
        return (
            <View style={{ flex: 1 }}>
                <CameraView
                    testID="camera"
                    style={{ flex: 1 }}
                    facing="back"
                    barcodeScannerSettings={{ barcodeTypes: [ 'qr' ] }}
                    onBarcodeScanned={onBarcodeScanned}
                />
                <View style={{ position: 'absolute', bottom: Spacing.large, left: Spacing.medium, right: Spacing.medium }}>
                    <Text style={[ CommonStyles.instruction, CommonStyles.notice, { color: '#ffffff', backgroundColor: 'rgba(0,0,0,0.6)' } ]}>
                        {t('Scan_Instruction')}
                    </Text>
                </View>
            </View>
        );
    }
    return (
        <ScrollView contentContainerStyle={[ CommonStyles.screen, CommonStyles.centered, { gap: Spacing.large } ]}>
            {isLoading ? <ActivityIndicator size="large" color={colors.primary} /> : null}
            {challenge ? <ChallengeSecretView challenge={challenge} /> : null}
            {errorMessage
            ? <Text style={[ CommonStyles.instruction, { color: colors.danger } ]}>{errorMessage}</Text>
            : null}
            {!isLoading
            ? <AppButton title={t('Scan_Again')} icon="qrcode-scan" variant="outline" onPress={scanAgain} />
            : null}
        </ScrollView>
    );
};

export default ScanScreen;
