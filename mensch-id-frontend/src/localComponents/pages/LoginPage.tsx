import { useEffect, useState } from 'react';
import { Button, Col, Row } from 'react-bootstrap';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { LoginProvider } from '../types/enums.d';
import { Models } from '../types/models';

interface LoginPageProps {
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}

export const LoginPage = (props: LoginPageProps) => {

    const [ isCheckingLoginStatus, setIsCheckingLoginStatus ] = useState<boolean>(true);
    const [ isLoggingIn, setIsLoggingIn ] = useState<boolean>(false);

    useEffect(() => {
        const checkLoginStatus = async () => {
            try {
                const response = await apiClient.instance!.get('api/accounts/accesstoken', {});
                const authenticationResult = await response.json() as Models.AuthenticationResult;
                if(authenticationResult.isAuthenticated) {
                    props.onLoggedIn(authenticationResult);
                }
            } catch {
                // Ignore
            } finally {
                setIsCheckingLoginStatus(false);
            }
        };
        checkLoginStatus();
    }, []);

    const login = async (loginProvider: LoginProvider) => {
        setIsLoggingIn(true);
        const redirectUrl = encodeURIComponent(`https://${window.location.host}/login`);
        const actionUrl = apiClient.instance!.buildUrl(`api/accounts/login/${loginProvider}?redirectUrl=${redirectUrl}`, {});
        window.location.href = actionUrl;
    }

    if(isCheckingLoginStatus) {
        return (<h3>{resolveText("CheckingLoginStatus")}</h3>);
    }

    return (
        <>
            <h1>{resolveText("Login")}</h1>
            <Row>
                <Col xs="auto">
                    <Button
                        variant="outline-secondary"
                        onClick={() => login(LoginProvider.Google)}
                        disabled={isLoggingIn}
                    >
                        <i className='fa fa-google' />
                    </Button>
                </Col>
                <Col xs="auto">
                    <Button
                        variant="outline-secondary"
                        onClick={() => login(LoginProvider.Twitter)}
                        disabled={isLoggingIn}
                    >
                        <i className='fa fa-twitter' />
                    </Button>
                </Col>
                <Col xs="auto">
                    <Button
                        variant="outline-secondary"
                        onClick={() => login(LoginProvider.Microsoft)}
                        disabled={isLoggingIn}
                    >
                        <i className='fa fa-windows' />
                    </Button>
                </Col>
                <Col xs="auto">
                    <Button
                        variant="outline-secondary"
                        onClick={() => login(LoginProvider.Facebook)}
                        disabled={isLoggingIn}
                    >
                        <i className='fa fa-facebook' />
                    </Button>
                </Col>
                <Col />
            </Row>
        </>
    );

}