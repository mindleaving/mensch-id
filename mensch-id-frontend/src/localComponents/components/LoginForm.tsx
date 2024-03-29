import { useState } from 'react';
import { ButtonGroup, Button, Card } from 'react-bootstrap';
import { Center } from '../../sharedCommonComponents/components/Center';
import { HorizontalLineWithText } from '../../sharedCommonComponents/components/HorizontalLineWithText';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { LoginProvider } from '../types/enums';
import { Models } from '../types/models';
import { ExternalLogins } from './ExternalLogins';
import { LocalLoginForm } from './LocalLoginForm';
import { RegistrationForm } from './RegistrationForm';

interface LoginFormProps {
    availableExternalProviders: LoginProvider[];
    onLocalLogin: (isLoggedInResponse: Models.IsLoggedInResponse) => void;
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
            <ExternalLogins
                availableExternalProviders={props.availableExternalProviders}
                onProviderSelected={props.onExternalProviderSelected}
            />
            <HorizontalLineWithText text={resolveText("or")} />
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