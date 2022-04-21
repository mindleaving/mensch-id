import React, { FormEvent, useState } from 'react';
import { Form, FormCheck, FormGroup } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { RowFormGroup } from '../../sharedCommonComponents/components/RowFormGroup';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../types/models';
import { NotificationManager } from 'react-notifications';
import { Center } from '../../sharedCommonComponents/components/Center';

interface RegistrationFormProps {
    onRegistering?: (loginInformation: Models.LoginInformation) => Promise<void>;
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}

export const RegistrationForm = (props: RegistrationFormProps) => {

    const [ email, setEmail ] = useState<string>('');
    const [ password, setPassword ] = useState<string>('');
    const [ passwordRepeat, setPasswordRepeat ] = useState<string>('');
    const [ acceptPrivacy, setAcceptPrivacy ] = useState<boolean>(false);
    const [ acceptTermsOfService, setAcceptTermsOfService ] = useState<boolean>(false);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    const register = async (e?: FormEvent) => {
        e?.preventDefault();
        if(password !== passwordRepeat) {
            NotificationManager.error(resolveText("Register_PasswordsDoNotMatch"));
            return;
        }
        setIsSubmitting(true);
        const loginInformation: Models.LoginInformation = {
            email: email,
            password: password,
            registerIfNotExists: true
        };
        if(props.onRegistering) {
            await props.onRegistering(loginInformation);
            setIsSubmitting(false);
        }
        else {
            await sendPostRequest(
                'api/accounts/login',
                resolveText("Register_CouldNotRegister"),
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
        <Form onSubmit={register}>
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
            <RowFormGroup required
                type='password'
                label={resolveText("PasswordRepeat")}
                value={passwordRepeat}
                onChange={setPasswordRepeat}
            />
            <FormGroup>
                <FormCheck required
                    label={resolveText("Register_AcceptPrivacy")}
                    onChange={(e:any) => setAcceptPrivacy(e.target.checked)}
                />
            </FormGroup>
            <FormGroup>
                <FormCheck required
                    label={resolveText("Register_AcceptTermsOfService")}
                    checked={acceptTermsOfService}
                    onChange={(e:any) => setAcceptTermsOfService(e.target.checked)}
                />
            </FormGroup>
            <Center>
                <AsyncButton
                    type='submit'
                    className='mt-3'
                    activeText={"Register"}
                    executingText={"Submitting..."}
                    isExecuting={isSubmitting}
                    disabled={!acceptPrivacy || !acceptTermsOfService}
                />
            </Center>
        </Form>
    );

}