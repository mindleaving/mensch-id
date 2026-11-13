import '../i18n';
import { StatusBar } from 'expo-status-bar';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { AuthProvider } from '../auth/AuthProvider';
import { AppNavigator } from '../components/AppNavigator';

const RootLayout = () => {
    return (
        <SafeAreaProvider>
            <AuthProvider>
                <StatusBar style="auto" />
                <AppNavigator />
            </AuthProvider>
        </SafeAreaProvider>
    );
};

export default RootLayout;
