import React, { useState } from 'react';
import { Row, Col, ButtonGroup, Button } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { LoginProvider } from '../types/enums.d';
import { Models } from '../types/models';
import { ExternalLogins } from './ExternalLogins';
import { LocalLoginForm } from './LocalLoginForm';
import { RegistrationForm } from './RegistrationForm';

interface LoginFormProps {
    availableExternalProviders: LoginProvider[];
    onLocalLogin: (authenticationResult: Models.AuthenticationResult) => void;
    onLocalLoggingIn?: (loginInformation: Models.LoginInformation) => Promise<void>;
    onExternalProviderSelected: (loginProvider: LoginProvider) => void;
}

enum LocalLoginMode {
    Login = "Login",
    Register = "Register"
}
export const LoginForm = (props: LoginFormProps) => {

    const [ localLoginMode, setLocalLoginMode ] = useState<LocalLoginMode>(LocalLoginMode.Login);

    return (
        <>
            <h1>{resolveText("Login")}</h1>
            <div className='mt-3' />
            <ExternalLogins
                availableExternalProviders={props.availableExternalProviders}
                onProviderSelected={props.onExternalProviderSelected}
            />
            <hr className='my-3' />
            <Row className='mb-3'>
                <Col />
                <Col xs="auto">
                    <ButtonGroup>
                        <Button 
                            variant={localLoginMode === LocalLoginMode.Register ? 'primary' : 'outline-dark'}
                            onClick={() => setLocalLoginMode(LocalLoginMode.Register)}
                        >
                            {resolveText("Register")}
                        </Button>
                        <Button 
                            variant={localLoginMode === LocalLoginMode.Login ? 'primary' : 'outline-dark'}
                            onClick={() => setLocalLoginMode(LocalLoginMode.Login)}
                        >
                            {resolveText("Login")}
                        </Button>
                    </ButtonGroup>
                </Col>
                <Col />
            </Row>
            {localLoginMode === LocalLoginMode.Login
            ? <LocalLoginForm
                onLoggingIn={props.onLocalLoggingIn}
                onLoggedIn={props.onLocalLogin}
            />
            : <RegistrationForm
                onRegistering={props.onLocalLoggingIn}
                onLoggedIn={props.onLocalLogin}
            />}
        </>
    );

}