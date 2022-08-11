import { FormEvent, useState } from 'react';
import { Alert, Form, FormControl, FormGroup, FormLabel } from 'react-bootstrap';
import { useSearchParams } from 'react-router-dom';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../types/models';
import { NotificationManager } from 'react-notifications';
import { StoreButton } from '../../sharedCommonComponents/components/StoreButton';

interface ResetPasswordPageProps {
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}

export const ResetPasswordPage = (props: ResetPasswordPageProps) => {

    const [ q ] = useSearchParams();
    const accountId = q.get("accountId");
    const resetToken = q.get("token");
    const [ password, setPassword ] = useState<string>('');
    const [ passwordRepeat, setPasswordRepeat ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    const resetPassword = async (e?: FormEvent) => {
        e?.preventDefault();
        if(!accountId || !resetToken) {
            return;
        }
        if(password !== passwordRepeat) {
            NotificationManager.error(resolveText("ResetPassword_PasswordsDoNotMatch"));
            return;
        }
        setIsSubmitting(true);
        const body: Models.ResetPasswordBody = {
            accountId: accountId,
            password: password,
            resetToken: resetToken
        };
        await sendPostRequest(
            `api/accounts/reset-password`,
            resolveText("ResetPassword_CouldNotReset"),
            body,
            async response => {
                const authenticationResult = await response.json() as Models.AuthenticationResult;
                if(authenticationResult.isAuthenticated) {
                    NotificationManager.success(resolveText("ResetPassword_SuccessfullyChanged"));
                    NotificationManager.info(resolveText("Redirecting..."));
                    props.onLoggedIn(authenticationResult);
                }
            },
            () => setIsSubmitting(false)
        );
    }

    if(!accountId || !resetToken) {
        return (
            <Alert variant="danger">
                {resolveText("ResetPassword_MissingAccountIdOrToken")}
            </Alert>
        );
    }

    return (
        <>
            <h1>{resolveText("ResetPassword")}</h1>
            <Form onSubmit={resetPassword}>
                <FormGroup>
                    <FormLabel>{resolveText("ResetPassword_NewPassword")}</FormLabel>
                    <FormControl
                        type="password"
                        value={password}
                        onChange={(e:any) => setPassword(e.target.value)}
                        isInvalid={password.length === 0}
                    />
                </FormGroup>
                <FormGroup>
                    <FormLabel>{resolveText("ResetPassword_NewPasswordRepeat")}</FormLabel>
                    <FormControl
                        type="password"
                        value={passwordRepeat}
                        onChange={(e:any) => setPasswordRepeat(e.target.value)}
                        isInvalid={passwordRepeat.length === 0 || passwordRepeat !== password}
                    />
                </FormGroup>
                <StoreButton
                    isStoring={isSubmitting}
                />
            </Form>
        </>
    );

}