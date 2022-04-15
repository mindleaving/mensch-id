import { useEffect, useState } from 'react';
import { Button, Col, Row } from 'react-bootstrap';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { buildLoadObjectFunc } from '../../sharedCommonComponents/helpers/LoadingHelpers';
import { LoginProvider } from '../types/enums.d';
import { ViewModels } from '../types/viewModels';
import { NotificationManager } from 'react-notifications';
import { Models } from '../types/models';

interface LinkAccountPageProps {}

export const LinkAccountPage = (props: LinkAccountPageProps) => {

    const [ isLoadingLoginProviders, setIsLoadingLoginProviders ] = useState<boolean>(true);
    const [ existingLoginProviders, setExistingLoginProviders ] = useState<LoginProvider[]>([]);

    useEffect(() => {
        const loadExistingLoginProviders = buildLoadObjectFunc<ViewModels.ProfileViewModel>(
            'api/profiles/me', {},
            resolveText("LinkAccount_CouldNotLoadExistingLoginProviders"),
            vm => setExistingLoginProviders(vm.loginProviders),
            () => setIsLoadingLoginProviders(false)
        );
        loadExistingLoginProviders();
    }, []);

    const linkAccount = async (loginProvider: LoginProvider) => {
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

    if(isLoadingLoginProviders) {
        return (<h3>{resolveText("Loading...")}</h3>);
    }
    return (
        <>
            <h1>{resolveText("LinkAnotherAccount")}</h1>
            <Row>
                {!existingLoginProviders.includes(LoginProvider.Google)
                ? <Col xs="auto">
                    <Button
                        size="lg"
                        variant="outline-dark"
                        onClick={() => linkAccount(LoginProvider.Google)}
                    >
                        <i className='fa fa-google' />
                    </Button>
                </Col>
                : null}
                {!existingLoginProviders.includes(LoginProvider.Twitter)
                ? <Col xs="auto">
                    <Button
                        size="lg"
                        variant="outline-dark"
                        onClick={() => linkAccount(LoginProvider.Twitter)}
                    >
                        <i className='fa fa-twitter' />
                    </Button>
                </Col>
                : null}
                {!existingLoginProviders.includes(LoginProvider.Microsoft)
                ? <Col xs="auto">
                    <Button
                        size="lg"
                        variant="outline-dark"
                        onClick={() => linkAccount(LoginProvider.Microsoft)}
                    >
                        <i className='fa fa-windows' />
                    </Button>
                </Col>
                : null}
                {!existingLoginProviders.includes(LoginProvider.Facebook)
                ? <Col xs="auto">
                    <Button
                        size="lg"
                        variant="outline-dark"
                        onClick={() => linkAccount(LoginProvider.Facebook)}
                    >
                        <i className='fa fa-facebook' />
                    </Button>
                </Col>
                : null}
                <Col />
            </Row>
        </>
    );

}