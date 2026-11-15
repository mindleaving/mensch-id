import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import { Models } from '../../types/models';
import { LoginForm } from '../../components/LoginForm';
import { showInfoAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { buildLoadObjectFunc } from '../../../sharedCommonComponents/helpers/LoadingHelpers';
import { ViewModels } from '../../types/viewModels';
import { buildExternalLoginUrl, getValidAuthorizeReturnUrl } from '../../helpers/LoginHelpers';

interface LoginPageProps {
    onLoggedIn: (isLoggedInResponse: Models.IsLoggedInResponse) => void;
}
export const LoginPage = (props: LoginPageProps) => {

    const [ loginProviders, setLoginProviders ] = useState<ViewModels.LoginProviderViewModel[]>([]);
    const [ searchParams ] = useSearchParams();
    const authorizeReturnUrl = useMemo(() => getValidAuthorizeReturnUrl(searchParams.get('returnUrl')), [ searchParams ]);

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
        const redirectUrl = authorizeReturnUrl ?? `https://${window.location.host}/login/redirect`;
        window.location.href = buildExternalLoginUrl(loginProviderId, redirectUrl, false);
    };

    const onLocalLogin = (isLoggedInResponse: Models.IsLoggedInResponse) => {
        if(authorizeReturnUrl) {
            showInfoAlert(resolveText("Redirecting..."));
            window.location.href = authorizeReturnUrl;
            return;
        }
        props.onLoggedIn(isLoggedInResponse);
    };

    return (
    <>
        <h1>{resolveText("Login")}</h1>
        {authorizeReturnUrl
        ? <p className='lead'>{resolveText("Login_ForApp")}</p>
        : null}
        <div className='mt-3' />
        <LoginForm
            availableExternalProviders={loginProviders}
            onExternalProviderSelected={externalLogin}
            onLocalLogin={onLocalLogin}
        />
    </>
    );
};
export default LoginPage;
