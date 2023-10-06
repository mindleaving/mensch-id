import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../../../sharedCommonComponents/communication/ApiClient';
import { showSuccessAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import { buildLoadObjectFunc } from '../../../sharedCommonComponents/helpers/LoadingHelpers';
import { sendPostRequest } from '../../../sharedCommonComponents/helpers/StoringHelpers';
import { LoginForm } from '../../components/LoginForm';
import { LoginProvider } from '../../types/enums.d';
import { Models } from '../../types/models';
import { ViewModels } from '../../types/viewModels';

interface LinkAccountPageProps {
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
            undefined,
            () => setIsLoadingLoginProviders(false)
        );
        loadExistingLoginProviders();
    }, []);

    const linkExternalAccount = async (loginProvider: LoginProvider) => {
        const redirectUrl = encodeURIComponent(`https://${window.location.host}/linkaccount/finish`);
        const actionUrl = apiClient.instance!.buildUrl(`api/accounts/login/${loginProvider}?redirectUrl=${redirectUrl}`, {});
        window.location.href = actionUrl;
    }

    const linkLocalAccount = async (loginInformation: Models.LoginInformation) => {
        await sendPostRequest(
            'api/accounts/link/local', {},
            resolveText("LinkAccount_CouldNotLink"),
            loginInformation,
            () => {
                showSuccessAlert(resolveText("LinkAccount_SuccessfullyLinked"));
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