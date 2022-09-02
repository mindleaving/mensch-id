import React, { FormEvent, useState } from 'react';
import { Form, FormGroup, FormLabel, FormControl } from 'react-bootstrap';
import { StoreButton } from '../../sharedCommonComponents/components/StoreButton';
import { showSuccessAlert } from '../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../types/models';

interface ChangePasswordFormProps {
    accountId: string;
    onPasswordChanged?: () => void;
}

export const ChangePasswordForm = (props: ChangePasswordFormProps) => {

    const [ currentPassword, setCurrentPassword ] = useState<string>('');
    const [ newPassword, setNewPassword ] = useState<string>('');
    const [ newPasswordRepeat, setNewPasswordRepeat ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    const changePassword = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsSubmitting(true);
        const body: Models.ChangePasswordRequest = {
            accountId: props.accountId,
            currentPassword: currentPassword, 
            newPassword: newPassword
        };
        await sendPostRequest(
            `api/accounts/${props.accountId}/change-password`,
            resolveText("Account_CouldNotChangePassword"),
            body,
            () => {
                showSuccessAlert(resolveText("ResetPassword_SuccessfullyChanged"));
                if(props.onPasswordChanged) {
                    props.onPasswordChanged();
                }
            },
            undefined,
            () => setIsSubmitting(false)
        )
    }

    return (
        <Form onSubmit={changePassword}>
            <FormGroup>
                <FormLabel>{resolveText("Account_CurrentPassword")}</FormLabel>
                <FormControl
                    type='password'
                    value={currentPassword}
                    onChange={(e:any) => setCurrentPassword(e.target.value)}
                />
            </FormGroup>
            <FormGroup>
                <FormLabel>{resolveText("ResetPassword_NewPassword")}</FormLabel>
                <FormControl
                    type='password'
                    value={newPassword}
                    onChange={(e:any) => setNewPassword(e.target.value)}
                />
            </FormGroup>
            <FormGroup>
                <FormLabel>{resolveText("ResetPassword_NewPasswordRepeat")}</FormLabel>
                <FormControl
                    type='password'
                    value={newPasswordRepeat}
                    onChange={(e:any) => setNewPasswordRepeat(e.target.value)}
                    isInvalid={newPasswordRepeat !== newPassword}
                />
                <FormControl.Feedback type='invalid'>{resolveText("ResetPassword_PasswordsDoNotMatch")}</FormControl.Feedback>
            </FormGroup>
            <StoreButton
                isStoring={isSubmitting}
                disabled={newPassword.length === 0 || newPasswordRepeat !== newPassword}
            />
        </Form>
    );

}