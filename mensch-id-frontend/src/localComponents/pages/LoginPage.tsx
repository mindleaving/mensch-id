import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { LoginProvider } from '../types/enums.d';
import { NotificationManager } from 'react-notifications';
import { Models } from '../types/models';
import { LoginForm } from '../components/LoginForm';

interface LoginPageProps {
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}
export const LoginPage = (props: LoginPageProps) => {

    const externalLogin = async (loginProvider: LoginProvider) => {
        NotificationManager.info(resolveText("Redirecting..."));
        const redirectUrl = encodeURIComponent(`https://${window.location.host}/login/redirect`);
        const actionUrl = apiClient.instance!.buildUrl(`api/accounts/login/${loginProvider}?redirectUrl=${redirectUrl}`, {});
        window.location.href = actionUrl;
    }

    return (
        <LoginForm
            availableExternalProviders={Object.values(LoginProvider).filter(x => ![LoginProvider.Unknown, LoginProvider.LocalJwt].includes(x))}
            onExternalProviderSelected={externalLogin}
            onLocalLogin={props.onLoggedIn}
        />
    )
}