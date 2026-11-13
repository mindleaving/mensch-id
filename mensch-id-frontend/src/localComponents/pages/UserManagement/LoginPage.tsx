import { useEffect, useState } from 'react';
import { apiClient } from '../../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import { Models } from '../../types/models';
import { LoginForm } from '../../components/LoginForm';
import { showInfoAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { buildLoadObjectFunc } from '../../../sharedCommonComponents/helpers/LoadingHelpers';
import { ViewModels } from '../../types/viewModels';

interface LoginPageProps {
    onLoggedIn: (isLoggedInResponse: Models.IsLoggedInResponse) => void;
}
export const LoginPage = (props: LoginPageProps) => {

    const [ loginProviders, setLoginProviders ] = useState<ViewModels.LoginProviderViewModel[]>([]);

    useEffect(() => {
        const loadLoginProviders = buildLoadObjectFunc<ViewModels.LoginProviderViewModel[]>(
            'api/accounts/login-providers', {},
            resolveText("LoginProviders_CouldNotLoad"),
            setLoginProviders
        );
        loadLoginProviders();
    }, []);

    const externalLogin = async (loginProviderId: string) => {
        showInfoAlert(resolveText("Redirecting..."));
        const redirectUrl = encodeURIComponent(`https://${window.location.host}/login/redirect`);
        const actionUrl = apiClient.instance!.buildUrl(`api/accounts/login/${encodeURIComponent(loginProviderId)}?redirectUrl=${redirectUrl}`, {});
        window.location.href = actionUrl;
    }

    return (
    <>
        <h1>{resolveText("Login")}</h1>
        <div className='mt-3' />
        <LoginForm
            availableExternalProviders={loginProviders}
            onExternalProviderSelected={externalLogin}
            onLocalLogin={props.onLoggedIn}
        />
    </>
    )
}
export default LoginPage;