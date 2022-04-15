import { Button, Col, Row } from 'react-bootstrap';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { LoginProvider } from '../types/enums.d';
import { NotificationManager } from 'react-notifications';

interface LoginPageProps {}

export const LoginPage = (props: LoginPageProps) => {

    const login = async (loginProvider: LoginProvider) => {
        NotificationManager.info(resolveText("Redirecting..."));
        const redirectUrl = encodeURIComponent(`https://${window.location.host}/login/redirect`);
        const actionUrl = apiClient.instance!.buildUrl(`api/accounts/login/${loginProvider}?redirectUrl=${redirectUrl}`, {});
        window.location.href = actionUrl;
    }



    return (
        <>
            <h1>{resolveText("Login")}</h1>
            <Row className='mt-3'>
                <Col xs="auto">
                    <Button
                        size="lg"
                        variant="outline-dark"
                        onClick={() => login(LoginProvider.Google)}
                    >
                        <i className='fa fa-google' />
                    </Button>
                </Col>
                <Col xs="auto">
                    <Button
                        size="lg"
                        variant="outline-dark"
                        onClick={() => login(LoginProvider.Twitter)}
                    >
                        <i className='fa fa-twitter' />
                    </Button>
                </Col>
                <Col xs="auto">
                    <Button
                        size="lg"
                        variant="outline-dark"
                        onClick={() => login(LoginProvider.Microsoft)}
                    >
                        <i className='fa fa-windows' />
                    </Button>
                </Col>
                <Col xs="auto">
                    <Button
                        size="lg"
                        variant="outline-dark"
                        onClick={() => login(LoginProvider.Facebook)}
                    >
                        <i className='fa fa-facebook' />
                    </Button>
                </Col>
                <Col />
            </Row>
        </>
    );

}