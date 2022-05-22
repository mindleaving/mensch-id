import { FormEvent, useContext, useState } from 'react';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import UserContext from '../contexts/UserContext';
import { Models } from '../types/models';
import { NotificationManager } from 'react-notifications';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { AuthenticationErrorType } from '../types/enums.d';
import { useNavigate } from 'react-router-dom';
import { Button, Col, Form, FormGroup, Row } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { Center } from '../../sharedCommonComponents/components/Center';
import { RowFormGroup } from '../../sharedCommonComponents/components/RowFormGroup';

interface LocalLoginFormProps {
    onSubmit?: (loginInformation: Models.LoginInformation) => Promise<void>;
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}

export const LocalLoginForm = (props: LocalLoginFormProps) => {

    const { profileData } = useContext(UserContext);
    const [ emailOrMenschID, setEmailOrMenschID ] = useState<string>(profileData?.id ?? '');
    const [ password, setPassword ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const navigate = useNavigate();

    const login = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsSubmitting(true);
        const loginInformation: Models.LoginInformation = {
            emailOrMenschId: emailOrMenschID,
            password: password
        };
        if(props.onSubmit) {
            try {
                await props.onSubmit(loginInformation);
            } finally {
                setIsSubmitting(false);
            }
        } else {
            try {
                const response = await apiClient.instance!.post('api/accounts/login', {}, loginInformation, { handleError: false });
                const authenticationResult = await response.json() as Models.AuthenticationResult;
                if(!authenticationResult) {
                    throw new Error("Could not log in");
                }
                if(authenticationResult.isAuthenticated) {
                    props.onLoggedIn(authenticationResult);
                } else if(authenticationResult.error === AuthenticationErrorType.EmailNotVerified && emailOrMenschID.includes('@')) {
                    navigate(`/verify-email?email=${encodeURIComponent(emailOrMenschID)}`);
                } else {
                    throw new Error("Could not log in");
                }
            } catch {
                NotificationManager.error(resolveText("Login_CouldNotLogIn"));
            } finally {
                setIsSubmitting(false);
            }
        }
    }

    return (
        <Form onSubmit={login}>
            <RowFormGroup required
                label={`웃ID / ${resolveText("Email")}`}
                value={emailOrMenschID}
                onChange={setEmailOrMenschID}
            />
            <RowFormGroup required
                type='password'
                label={resolveText("Password")}
                value={password}
                onChange={setPassword}
            />
            <Row>
                <Col></Col>
                <Col xs="auto">
                    <Button variant="link" onClick={() => navigate("/request-password-reset")}>{resolveText("Login_ForgotPassword")}</Button>
                </Col>
            </Row>
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