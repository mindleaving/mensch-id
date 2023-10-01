import React, { FormEvent, useState } from 'react';
import { Form, FormControl, FormGroup, FormLabel } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { StoreButton } from '../../../sharedCommonComponents/components/StoreButton';
import { showSuccessAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../../types/models';

interface RequestPasswordResetPageProps {}

export const RequestPasswordResetPage = (props: RequestPasswordResetPageProps) => {

    const [ email, setEmail ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const navigate = useNavigate();

    const submit = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsSubmitting(true);
        const body: Models.ResetPasswordRequest = {
            email: email
        };
        await sendPostRequest(
            `api/accounts/request-password-reset`, {},
            resolveText("ResetPassword_CouldNotRequest"),
            body,
            () => {
                showSuccessAlert(resolveText("ResetPassword_SuccessfullySubmittedRequest"));
                navigate("/");
            }
        )
    }
    return (
        <>
            <h1>{resolveText("RequestPasswordReset")}</h1>
            <Form onSubmit={submit}>
                <FormGroup>
                    <FormLabel>{resolveText("Email")}</FormLabel>
                    <FormControl
                        value={email}
                        onChange={(e:any) => setEmail(e.target.value)}
                    />
                </FormGroup>
                <StoreButton
                    isStoring={isSubmitting}
                />
            </Form>
        </>
    );

}
export default RequestPasswordResetPage;