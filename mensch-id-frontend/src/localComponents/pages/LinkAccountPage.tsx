import { useEffect, useState } from 'react';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { buildLoadObjectFunc } from '../../sharedCommonComponents/helpers/LoadingHelpers';
import { LoginProvider } from '../types/enums.d';
import { ViewModels } from '../types/viewModels';
import { NotificationManager } from 'react-notifications';
import { Models } from '../types/models';
import { LoginForm } from '../components/LoginForm';
import { useNavigate } from 'react-router-dom';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';

interface LinkAccountPageProps {
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}

export const LinkAccountPage = (props: LinkAccountPageProps) => {

    const [ isLoadingLoginProviders, setIsLoadingLoginProviders ] = useState<boolean>(true);
    const [ existingLoginProviders, setExistingLoginProviders ] = useState<LoginProvider[]>([]);
    const navigate = useNavigate();

    useEffect(() => {
        const loadExistingLoginProviders = buildLoadObjectFunc<ViewModels.ProfileViewModel>(
            'api/profiles/me', {},
            resolveText("LinkAccount_CouldNotLoadExistingLoginProviders"),
            vm => setExistingLoginProviders(vm.loginProviders),
            () => setIsLoadingLoginProviders(false)
        );
        loadExistingLoginProviders();
    }, []);

    const linkExternalAccount = async (loginProvider: LoginProvider) => {
        if(!apiClient.instance!.accessToken) {
            try {
                const response = await apiClient.instance!.get('api/accounts/accesstoken', {});
                const authenticationResult = await response.json() as Models.AuthenticationResult;
                if(!authenticationResult.isAuthenticated) {
                    throw new Error("Not logged in");
                }
                apiClient.instance!.setAccessToken(authenticationResult.accessToken!);
                sessionStorage.setItem("accesstoken", authenticationResult.accessToken!);
            } catch {
                NotificationManager.error(resolveText("LinkAccount_CouldNotSetupLink"));
                return;
            }
        }
        const redirectUrl = encodeURIComponent(`https://${window.location.host}/linkaccount/finish`);
        const actionUrl = apiClient.instance!.buildUrl(`api/accounts/login/${loginProvider}?redirectUrl=${redirectUrl}`, {});
        window.location.href = actionUrl;
    }

    const linkLocalAccount = async (loginInformation: Models.LoginInformation) => {
        await sendPostRequest(
            'api/accounts/link/local',
            resolveText("LinkAccount_CouldNotLink"),
            loginInformation,
            () => {
                NotificationManager.success(resolveText("LinkAccount_SuccessfullyLinked"));
                navigate("/me");
            }
        );
    }

    if(isLoadingLoginProviders) {
        return (<h3>{resolveText("Loading...")}</h3>);
    }

    const availableExternalProviders = Object.values(LoginProvider).filter(x => !existingLoginProviders.includes(x));
    return (
        <>
            <h1>{resolveText("LinkAnotherAccount")}</h1>
            <LoginForm
                availableExternalProviders={availableExternalProviders}
                onExternalProviderSelected={linkExternalAccount}
                onLocalLogin={() => {}}
                onLocalLoggingIn={linkLocalAccount}
            />
        </>
    );

}