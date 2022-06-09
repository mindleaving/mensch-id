import { useState } from 'react';
import { ButtonGroup, Button, Card } from 'react-bootstrap';
import { Center } from '../../sharedCommonComponents/components/Center';
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
            <Center className="mb-3">
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
            </Center>
            <Center>
                <Card style={{ width: '600px'}}>
                    <Card.Body>
                        {localLoginMode === LocalLoginMode.Login
                        ? <LocalLoginForm
                            onSubmit={props.onLocalLoggingIn}
                            onLoggedIn={props.onLocalLogin}
                        />
                        : <RegistrationForm
                            onLoggedIn={props.onLocalLogin}
                        />}
                    </Card.Body>
                </Card>
            </Center>
        </>
    );

}