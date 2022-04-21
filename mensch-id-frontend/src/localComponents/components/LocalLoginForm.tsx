import React, { FormEvent, useState } from 'react';
import { Form } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { Center } from '../../sharedCommonComponents/components/Center';
import { RowFormGroup } from '../../sharedCommonComponents/components/RowFormGroup';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../types/models';

interface LocalLoginFormProps {
    onLoggingIn?: (loginInformation: Models.LoginInformation) => Promise<void>;
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}

export const LocalLoginForm = (props: LocalLoginFormProps) => {

    const [ email, setEmail ] = useState<string>('');
    const [ password, setPassword ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    const login = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsSubmitting(true);
        const loginInformation: Models.LoginInformation = {
            email: email,
            password: password
        };
        if(props.onLoggingIn) {
            try {
                await props.onLoggingIn(loginInformation);
            } finally {
                setIsSubmitting(false);
            }
        } else {
            await sendPostRequest(
                'api/accounts/login',
                resolveText("Login_CouldNotLogIn"),
                loginInformation,
                async response => {
                    const authenticationResult = await response.json() as Models.AuthenticationResult;
                    props.onLoggedIn(authenticationResult);
                },
                () => setIsSubmitting(false)
            );
        }
    }

    return (
        <Form onSubmit={login}>
            <RowFormGroup required
                label={resolveText("Email")}
                value={email}
                onChange={setEmail}
                isValid={email.includes('@')}
            />
            <RowFormGroup required
                type='password'
                label={resolveText("Password")}
                value={password}
                onChange={setPassword}
            />
            <Center>
                <AsyncButton
                    type='submit'
                    activeText={"Login"}
                    executingText={"Submitting..."}
                    isExecuting={isSubmitting}
                />
            </Center>

        </Form>
    );

}