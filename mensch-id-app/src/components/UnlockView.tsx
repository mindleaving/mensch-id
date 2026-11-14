import { useEffect, useRef, useState } from 'react';
import { Pressable, Text } from 'react-native';
import { useTranslation } from 'react-i18next';
import { SafeAreaView } from 'react-native-safe-area-context';
import { PinEntry } from './PinEntry';
import { useAppLock } from '../lock/AppLockContext';
import { useAuth } from '../auth/AuthContext';
import { FailedPinAttemptsBeforeWarning, MaximumFailedPinAttempts } from '../lock/AppLockService';
import { CommonStyles, Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';
import { useIsAppActive } from '../hooks/useIsAppActive';

interface UnlockViewProps {}

export const UnlockView = (props: UnlockViewProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const appLock = useAppLock();
    const auth = useAuth();
    const [ isPinRequiredAfterBiometrics, setIsPinRequiredAfterBiometrics ] = useState<boolean>(false);
    const [ errorMessage, setErrorMessage ] = useState<string>();
    const hasPromptedBiometrics = useRef<boolean>(false);
    const isAppActive = useIsAppActive();
    const canUseBiometrics = appLock.isBiometricsEnabled && !isPinRequiredAfterBiometrics;

    const unlockWithBiometrics = async () => {
        const result = await appLock.unlockWithBiometrics();
        if(result === 'pinRequired') {
            setIsPinRequiredAfterBiometrics(true);
        }
    };

    useEffect(() => {
        if(!isAppActive || !appLock.isBiometricsEnabled || hasPromptedBiometrics.current) {
            return;
        }
        hasPromptedBiometrics.current = true;
        const promptBiometrics = async () => {
            const result = await appLock.unlockWithBiometrics();
            if(result === 'pinRequired') {
                setIsPinRequiredAfterBiometrics(true);
            }
        };
        promptBiometrics();
    }, [ appLock, isAppActive ]);

    const unlockWithPin = async (pin: string) => {
        const result = await appLock.unlockWithPin(pin);
        if(result === 'invalid') {
            setErrorMessage(t('Pin_Wrong'));
        }
    };

    const failedAttempts = MaximumFailedPinAttempts - appLock.remainingPinAttempts;
    const attemptsWarning = failedAttempts >= FailedPinAttemptsBeforeWarning
        ? t('Pin_RemainingAttempts', { count: appLock.remainingPinAttempts })
        : undefined;
    const description = isPinRequiredAfterBiometrics
        ? t('Lock_PinRequiredAfterBiometrics')
        : t('Lock_Description');

    return (
        <SafeAreaView style={{ flex: 1, backgroundColor: colors.background }}>
            <PinEntry
                title={t('Lock_Title')}
                description={description}
                errorMessage={[ errorMessage, attemptsWarning ].filter(Boolean).join(' ') || undefined}
                submitTitle={t('Lock_Unlock')}
                onSubmit={unlockWithPin}
                extraKeyIcon={canUseBiometrics ? 'fingerprint' : undefined}
                extraKeyLabel={t('Lock_UseBiometrics')}
                onExtraKeyPressed={canUseBiometrics ? unlockWithBiometrics : undefined}
                footer={
                    <Pressable
                        accessibilityRole="button"
                        testID="forgot-pin"
                        onPress={auth.logout}
                        style={{ padding: Spacing.small }}
                    >
                        <Text style={[ CommonStyles.instruction, { color: colors.primary } ]}>{t('Lock_ForgotPin')}</Text>
                    </Pressable>
                }
            />
        </SafeAreaView>
    );
};
