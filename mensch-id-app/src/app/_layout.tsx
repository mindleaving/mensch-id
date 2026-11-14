import '../i18n';
import { useEffect } from 'react';
import { Platform } from 'react-native';
import * as ScreenCapture from 'expo-screen-capture';
import { StatusBar } from 'expo-status-bar';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { AuthProvider } from '../auth/AuthProvider';
import { AppNavigator } from '../components/AppNavigator';
import { AppLockProvider } from '../lock/AppLockProvider';

const RootLayout = () => {

    ScreenCapture.usePreventScreenCapture();

    useEffect(() => {
        if(Platform.OS !== 'ios') {
            return;
        }
        ScreenCapture.enableAppSwitcherProtectionAsync().catch(() => undefined);
    }, []);

    return (
        <SafeAreaProvider>
            <AuthProvider>
                <AppLockProvider>
                    <StatusBar style="auto" />
                    <AppNavigator />
                </AppLockProvider>
            </AuthProvider>
        </SafeAreaProvider>
    );
};

export default RootLayout;
