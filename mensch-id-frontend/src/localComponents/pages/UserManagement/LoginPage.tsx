import { apiClient } from '../../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import { LoginProvider } from '../../types/enums.d';
import { Models } from '../../types/models';
import { LoginForm } from '../../components/LoginForm';
import { showInfoAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';

interface LoginPageProps {
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}
export const LoginPage = (props: LoginPageProps) => {

    const externalLogin = async (loginProvider: LoginProvider) => {
        showInfoAlert(resolveText("Redirecting..."));
        const redirectUrl = encodeURIComponent(`https://${window.location.host}/login/redirect`);
        const actionUrl = apiClient.instance!.buildUrl(`api/accounts/login/${loginProvider}?redirectUrl=${redirectUrl}`, {});
        window.location.href = actionUrl;
    }

    return (
    <>
        <h1>{resolveText("Login")}</h1>
        <div className='mt-3' />
        <LoginForm
            availableExternalProviders={Object.values(LoginProvider).filter(x => ![LoginProvider.Unknown, LoginProvider.LocalJwt].includes(x))}
            onExternalProviderSelected={externalLogin}
            onLocalLogin={props.onLoggedIn}
        />
    </>
    )
}
export default LoginPage;