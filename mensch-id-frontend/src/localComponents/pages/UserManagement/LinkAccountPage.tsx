import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { showErrorAlert, showSuccessAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import { buildLoadObjectFunc } from '../../../sharedCommonComponents/helpers/LoadingHelpers';
import { sendPostRequest } from '../../../sharedCommonComponents/helpers/StoringHelpers';
import { LoginForm } from '../../components/LoginForm';
import { ReauthenticationPrompt } from '../../components/ReauthenticationPrompt';
import { buildExternalLoginUrl, isReauthenticationRequired } from '../../helpers/LoginHelpers';
import { Models } from '../../types/models';
import { ViewModels } from '../../types/viewModels';

interface LinkAccountPageProps {
}

export const LinkAccountPage = (props: LinkAccountPageProps) => {

    const [ isLoadingLoginProviders, setIsLoadingLoginProviders ] = useState<boolean>(true);
    const [ existingLoginProviders, setExistingLoginProviders ] = useState<string[]>([]);
    const [ isLoadingAvailableLoginProviders, setIsLoadingAvailableLoginProviders ] = useState<boolean>(true);
    const [ loginProviders, setLoginProviders ] = useState<ViewModels.LoginProviderViewModel[]>([]);
    const [ isCheckingRecentAuthentication, setIsCheckingRecentAuthentication ] = useState<boolean>(true);
    const [ isRecentlyAuthenticated, setIsRecentlyAuthenticated ] = useState<boolean>(false);
    const navigate = useNavigate();

    useEffect(() => {
        const loadExistingLoginProviders = buildLoadObjectFunc<ViewModels.ProfileViewModel>(
            'api/profiles/me', {},
            resolveText("LinkAccount_CouldNotLoadExistingLoginProviders"),
            vm => setExistingLoginProviders(vm.loginProviders),
            undefined,
            () => setIsLoadingLoginProviders(false)
        );
        loadExistingLoginProviders();
        const loadLoginProviders = buildLoadObjectFunc<ViewModels.LoginProviderViewModel[]>(
            'api/accounts/login-providers', {},
            resolveText("LoginProviders_CouldNotLoad"),
            setLoginProviders,
            undefined,
            () => setIsLoadingAvailableLoginProviders(false)
        );
        loadLoginProviders();
        const checkRecentAuthentication = buildLoadObjectFunc<boolean>(
            'api/accounts/is-recently-authenticated', {},
            resolveText("Reauthentication_CouldNotCheck"),
            setIsRecentlyAuthenticated,
            undefined,
            () => setIsCheckingRecentAuthentication(false)
        );
        checkRecentAuthentication();
    }, []);

    const linkExternalAccount = async (loginProviderId: string) => {
        const redirectUrl = `https://${window.location.host}/linkaccount/finish`;
        window.location.href = buildExternalLoginUrl(loginProviderId, redirectUrl, true);
    }

    const linkLocalAccount = async (loginInformation: Models.LoginInformation) => {
        await sendPostRequest(
            'api/accounts/link/local', {},
            resolveText("LinkAccount_CouldNotLink"),
            loginInformation,
            async () => {
                showSuccessAlert(resolveText("LinkAccount_SuccessfullyLinked"));
                navigate("/me");
            },
            async response => {
                if(await isReauthenticationRequired(response)) {
                    setIsRecentlyAuthenticated(false);
                    return;
                }
                showErrorAlert(resolveText("LinkAccount_CouldNotLink"), await response?.text());
            }
        );
    }

    if(isLoadingLoginProviders || isLoadingAvailableLoginProviders || isCheckingRecentAuthentication) {
        return (<h3>{resolveText("Loading...")}</h3>);
    }

    if(!isRecentlyAuthenticated) {
        return (
            <>
                <h1>{resolveText("LinkAnotherAccount")}</h1>
                <ReauthenticationPrompt
                    availableExternalProviders={loginProviders}
                    returnPath="/linkaccount"
                    onReauthenticated={() => setIsRecentlyAuthenticated(true)}
                />
            </>
        );
    }

    const availableExternalProviders = loginProviders.filter(x => !existingLoginProviders.includes(x.id));
    return (
        <>
            <h1>{resolveText("LinkAnotherAccount")}</h1>
            <div className='mt-3' />
            <LoginForm
                availableExternalProviders={availableExternalProviders}
                onExternalProviderSelected={linkExternalAccount}
                onLocalLogin={() => {}}
                onLocalLoggingIn={linkLocalAccount}
            />
        </>
    );

}
export default LinkAccountPage;