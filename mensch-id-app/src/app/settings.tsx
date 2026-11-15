import { useState } from 'react';
import { ScrollView, Switch, Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { AppButton } from '../components/AppButton';
import { ChangePinFlow } from '../components/ChangePinFlow';
import { useAppLock } from '../lock/AppLockContext';
import { useThemeColors } from '../hooks/useThemeColors';
import { CommonStyles, Spacing } from '../styles/Theme';

const SettingsScreen = () => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const appLock = useAppLock();
    const [ isChangingPin, setIsChangingPin ] = useState<boolean>(false);
    const [ isUpdatingBiometrics, setIsUpdatingBiometrics ] = useState<boolean>(false);
    const [ message, setMessage ] = useState<string>();

    const toggleBiometrics = async (isEnabled: boolean) => {
        setIsUpdatingBiometrics(true);
        setMessage(undefined);
        try {
            const isChanged = await appLock.setBiometricsEnabled(isEnabled);
            if(!isChanged) {
                setMessage(t('Settings_BiometricsNotConfirmed'));
            }
        } finally {
            setIsUpdatingBiometrics(false);
        }
    };

    const finishPinChange = () => {
        setIsChangingPin(false);
        setMessage(t('Settings_PinChanged'));
    };

    if(isChangingPin) {
        return <ChangePinFlow onFinished={finishPinChange} />;
    }
    return (
        <ScrollView contentContainerStyle={[ CommonStyles.screen, { gap: Spacing.large } ]}>
            <View style={[ CommonStyles.notice, { backgroundColor: colors.surface, gap: Spacing.small } ]}>
                <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', gap: Spacing.medium }}>
                    <Text nativeID="biometrics-label" style={{ fontSize: 16, fontWeight: '600', color: colors.text, flexShrink: 1 }}>
                        {t('Settings_Biometrics')}
                    </Text>
                    <Switch
                        testID="biometrics-switch"
                        accessibilityLabelledBy="biometrics-label"
                        accessibilityLabel={t('Settings_Biometrics')}
                        value={appLock.isBiometricsEnabled}
                        disabled={!appLock.isBiometricsAvailable || isUpdatingBiometrics}
                        onValueChange={toggleBiometrics}
                    />
                </View>
                <Text style={{ fontSize: 14, color: colors.mutedText }}>
                    {appLock.isBiometricsAvailable ? t('Settings_BiometricsDescription') : t('Settings_BiometricsUnavailable')}
                </Text>
            </View>
            <View style={[ CommonStyles.notice, { backgroundColor: colors.surface, gap: Spacing.medium } ]}>
                <Text style={{ fontSize: 16, fontWeight: '600', color: colors.text }}>{t('Settings_Pin')}</Text>
                <Text style={{ fontSize: 14, color: colors.mutedText }}>{t('Settings_PinDescription')}</Text>
                <AppButton
                    title={t('Settings_ChangePin')}
                    icon="form-textbox-password"
                    variant="outline"
                    testID="change-pin"
                    onPress={() => {
                        setMessage(undefined);
                        setIsChangingPin(true);
                    }}
                />
            </View>
            {message
            ? <Text style={[ CommonStyles.instruction, { color: colors.text } ]}>{message}</Text>
            : null}
        </ScrollView>
    );
};

export default SettingsScreen;
