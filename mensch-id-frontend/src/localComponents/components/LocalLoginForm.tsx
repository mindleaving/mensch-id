import { FormEvent, useContext, useState } from 'react';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import UserContext from '../contexts/UserContext';
import { Models } from '../types/models';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { AuthenticationErrorType } from '../types/enums.d';
import { useNavigate } from 'react-router-dom';
import { Button, Col, Form, Row } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { Center } from '../../sharedCommonComponents/components/Center';
import { RowFormGroup } from '../../sharedCommonComponents/components/FormControls/RowFormGroup';
import { showErrorAlert } from '../../sharedCommonComponents/helpers/AlertHelpers';
import { ViewModels } from '../types/viewModels';

interface LocalLoginFormProps {
    onSubmit?: (loginInformation: Models.LoginInformation) => Promise<void>;
    onLoggedIn: (isLoggedInResponse: Models.IsLoggedInResponse) => void;
}

export const LocalLoginForm = (props: LocalLoginFormProps) => {

    const profileData = useContext(UserContext)! as ViewModels.ProfileViewModel;
    const [ username, setUsername ] = useState<string>(profileData?.id ?? '');
    const [ password, setPassword ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const navigate = useNavigate();

    const login = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsSubmitting(true);
        const loginInformation: Models.LoginInformation = {
            emailMenschIdOrUsername: username,
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
                const response = await apiClient.instance!.post('api/accounts/login', loginInformation, {}, { handleError: false });
                if(!response.ok) {
                    const error = await response.text() as AuthenticationErrorType;
                    if(error === AuthenticationErrorType.EmailNotVerified && username.includes('@')) {
                        navigate(`/verify-email?email=${encodeURIComponent(username)}`);
                    }
                    throw new Error("Could not log in");
                }
                const isLoggedInResponse = await response.json() as Models.IsLoggedInResponse;
                props.onLoggedIn(isLoggedInResponse);
            } catch {
                showErrorAlert(resolveText("Login_CouldNotLogIn"));
            } finally {
                setIsSubmitting(false);
            }
        }
    }

    return (
        <Form onSubmit={login}>
            <RowFormGroup required
                label={`웃ID / ${resolveText("Email")} / ${resolveText("Username")}`}
                id="username"
                name='username'
                value={username}
                onChange={setUsername}
            />
            <RowFormGroup required
                type='password'
                label={resolveText("Password")}
                id="current-password"
                name="current-password"
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