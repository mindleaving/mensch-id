import { Stack } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/AuthContext';
import { StatusScreen } from './StatusScreen';
import { PinSetupView } from './PinSetupView';
import { UnlockView } from './UnlockView';
import { useAppLock } from '../lock/AppLockContext';
import { useThemeColors } from '../hooks/useThemeColors';

export const AppNavigator = () => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const auth = useAuth();
    const appLock = useAppLock();

    if(auth.status === 'loading') {
        return <StatusScreen isLoading message={t('Loading')} />;
    }
    if(auth.status === 'unavailable') {
        return (
            <StatusScreen
                message={t('ServerUnavailable')}
                actionTitle={t('Retry')}
                onAction={auth.retryConnection}
            />
        );
    }
    const isLoggedIn = auth.status === 'loggedIn';
    if(isLoggedIn && appLock.status === 'loading') {
        return <StatusScreen isLoading message={t('Loading')} />;
    }
    if(isLoggedIn && appLock.status === 'setupRequired') {
        return <PinSetupView />;
    }
    if(isLoggedIn && appLock.status === 'locked') {
        return <UnlockView />;
    }
    return (
        <Stack
            screenOptions={{
                headerStyle: { backgroundColor: colors.background },
                headerTintColor: colors.text,
                contentStyle: { backgroundColor: colors.background }
            }}
        >
            <Stack.Protected guard={isLoggedIn}>
                <Stack.Screen name="index" options={{ title: t('AppName') }} />
                <Stack.Screen name="scan" options={{ title: t('Scan_Title') }} />
                <Stack.Screen name="create-challenge" options={{ title: t('CreateChallenge_Title') }} />
                <Stack.Screen name="settings" options={{ title: t('Settings_Title') }} />
            </Stack.Protected>
            <Stack.Protected guard={!isLoggedIn}>
                <Stack.Screen name="login" options={{ headerShown: false }} />
            </Stack.Protected>
        </Stack>
    );
};
