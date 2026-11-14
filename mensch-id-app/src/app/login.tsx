import { useEffect, useState } from 'react';
import { Image, ScrollView, Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useAuth } from '../auth/AuthContext';
import { LoginCancelledError } from '../auth/OidcClient';
import { AppButton } from '../components/AppButton';
import { Checkbox } from '../components/Checkbox';
import { useAppLock } from '../lock/AppLockContext';
import { getLoginProviderIcon, loadLoginProviders } from '../helpers/LoginProviderHelpers';
import { CommonStyles, Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';
import { ViewModels } from '../types/viewModels';

const LoginScreen = () => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const auth = useAuth();
    const appLock = useAppLock();
    const [ isPinResetRequested, setIsPinResetRequested ] = useState<boolean>(false);
    const [ loginProviders, setLoginProviders ] = useState<ViewModels.LoginProviderViewModel[]>([]);
    const [ hasProviderError, setHasProviderError ] = useState<boolean>(false);
    const [ activeLogin, setActiveLogin ] = useState<string>();
    const [ hasLoginError, setHasLoginError ] = useState<boolean>(false);

    useEffect(() => {
        loadLoginProviders()
            .then(setLoginProviders)
            .catch(() => setHasProviderError(true));
    }, []);

    const changePinResetRequest = (isRequested: boolean) => {
        setIsPinResetRequested(isRequested);
        appLock.setPinResetRequested(isRequested);
    };

    const login = async (loginKey: string, loginProviderId?: string) => {
        setActiveLogin(loginKey);
        setHasLoginError(false);
        try {
            await auth.login(loginProviderId);
        } catch (error) {
            if(!(error instanceof LoginCancelledError)) {
                setHasLoginError(true);
            }
        } finally {
            setActiveLogin(undefined);
        }
    };

    return (
        <SafeAreaView style={{ flex: 1, backgroundColor: colors.background }}>
            <ScrollView contentContainerStyle={[ CommonStyles.screen, CommonStyles.centered, { gap: Spacing.large } ]}>
                <Image
                    source={require('../../assets/icon.png')}
                    style={{ width: 120, height: 120, borderRadius: 24 }}
                    accessibilityIgnoresInvertColors
                />
                <Text style={[ CommonStyles.title, { color: colors.text } ]}>{t('Login_Title')}</Text>
                <View style={{ gap: Spacing.medium, alignSelf: 'stretch' }}>
                    <AppButton
                        title={t('Login_WithPassword')}
                        icon="form-textbox-password"
                        testID="login-password"
                        isLoading={activeLogin === 'password'}
                        disabled={!auth.isLoginAvailable || activeLogin !== undefined}
                        onPress={() => login('password')}
                    />
                    {loginProviders.map(loginProvider => (
                        <AppButton
                            key={loginProvider.id}
                            title={t('Login_WithProvider', { provider: loginProvider.displayName })}
                            icon={getLoginProviderIcon(loginProvider.id)}
                            variant="outline"
                            testID={`login-${loginProvider.id}`}
                            isLoading={activeLogin === loginProvider.id}
                            disabled={!auth.isLoginAvailable || activeLogin !== undefined}
                            onPress={() => login(loginProvider.id, loginProvider.id)}
                        />
                    ))}
                </View>
                <Checkbox
                    label={t('Login_ResetPin')}
                    isChecked={isPinResetRequested}
                    onChange={changePinResetRequest}
                    testID="reset-pin"
                />
                {hasProviderError
                ? <Text style={[ CommonStyles.instruction, { color: colors.mutedText } ]}>{t('LoginProviders_CouldNotLoad')}</Text>
                : null}
                {hasLoginError
                ? <Text style={[ CommonStyles.instruction, { color: colors.danger } ]}>{t('Login_Failed')}</Text>
                : null}
            </ScrollView>
        </SafeAreaView>
    );
};

export default LoginScreen;
