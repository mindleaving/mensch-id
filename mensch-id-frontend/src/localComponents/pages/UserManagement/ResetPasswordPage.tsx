import { FormEvent, useState } from 'react';
import { Alert, Form, FormControl, FormGroup, FormLabel } from 'react-bootstrap';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { AsyncButton } from '../../../sharedCommonComponents/components/AsyncButton';
import { showErrorAlert, showSuccessAlert, showInfoAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../../types/models';
import { AuthenticationErrorType } from '../../types/enums';

interface ResetPasswordPageProps {
    onLoggedIn: (isLoggedInResponse: Models.IsLoggedInResponse) => void;
}

export const ResetPasswordPage = (props: ResetPasswordPageProps) => {

    const [ q ] = useSearchParams();
    const accountId = q.get("accountId");
    const resetToken = q.get("token");
    const [ password, setPassword ] = useState<string>('');
    const [ passwordRepeat, setPasswordRepeat ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const navigate = useNavigate();

    const resetPassword = async (e?: FormEvent) => {
        e?.preventDefault();
        if(!accountId || !resetToken) {
            return;
        }
        if(password !== passwordRepeat) {
            showErrorAlert(resolveText("ResetPassword_PasswordsDoNotMatch"));
            return;
        }
        setIsSubmitting(true);
        const body: Models.ResetPasswordBody = {
            accountId: accountId,
            password: password,
            resetToken: resetToken
        };
        await sendPostRequest(
            `api/accounts/reset-password`, {},
            resolveText("ResetPassword_CouldNotReset"),
            body,
            async response => {
                const isLoggedInResponse = await response.json() as Models.IsLoggedInResponse;
                showSuccessAlert(resolveText("ResetPassword_SuccessfullyChanged"));
                showInfoAlert(resolveText("Redirecting..."));
                props.onLoggedIn(isLoggedInResponse);
            },
            async response => {
                if(response?.status === 401) {
                    showSuccessAlert(resolveText("ResetPassword_SuccessfullyChanged"));
                    try {
                        const error = await response?.json() as AuthenticationErrorType;
                        if(error === AuthenticationErrorType.EmailNotVerified) {
                            navigate(`/verify-email?accountId=${encodeURIComponent(accountId)}`);
                        } else {
                            navigate('/login');
                        }
                    } catch {
                        navigate('/login');
                    }
                    return;
                }
                showErrorAlert(resolveText("ResetPassword_CouldNotReset"));
                navigate('/login');
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
                    <FormControl required
                        type="password"
                        value={password}
                        onChange={(e:any) => setPassword(e.target.value)}
                        minLength={8}
                        isInvalid={password.length === 0}
                        id="new-password"
                        name="new-password"
                    />
                </FormGroup>
                <FormGroup>
                    <FormLabel>{resolveText("ResetPassword_NewPasswordRepeat")}</FormLabel>
                    <FormControl required
                        type="password"
                        value={passwordRepeat}
                        onChange={(e:any) => setPasswordRepeat(e.target.value)}
                        isInvalid={passwordRepeat.length === 0 || passwordRepeat !== password}
                        id="new-password2"
                        name="new-password2"
                    />
                </FormGroup>
                <AsyncButton
                    type='submit'
                    className='m-3'
                    activeText={resolveText("Submit")}
                    executingText={resolveText("Submitting...")}
                    isExecuting={isSubmitting}
                    disabled={password.length === 0 || passwordRepeat !== password}
                />
            </Form>
        </>
    );

}
export default ResetPasswordPage;