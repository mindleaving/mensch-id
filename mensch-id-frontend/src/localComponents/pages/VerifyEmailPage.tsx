import React, { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { Alert, Button } from 'react-bootstrap';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../types/models';

interface VerifyEmailPageProps {}

export const VerifyEmailPage = (props: VerifyEmailPageProps) => {

    const [ q ] = useSearchParams();
    const accountId = q.get("accountId");
    const email = q.get("email") ? decodeURIComponent(q.get("email")!) : null;
    const verificationToken = q.get("token");
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(true);
    const [ verificationEmailHasBeenSent, setVerificationEmailHasBeenSent ] = useState<boolean>(false);
    const [ isVerified, setIsVerified ] = useState<boolean>(false);

    const verifyEmail = async () => {
        if(!accountId || !verificationToken) {
            return;
        }
        setIsSubmitting(true);
        try {
            await apiClient.instance!.get(`api/accounts/${accountId}/verify-email`, { token: verificationToken });
            setIsVerified(true);
        } catch {
            setIsVerified(false);
        } finally {
            setIsSubmitting(false);
        }
    }

    useEffect(() => {  
        verifyEmail();
    }, []);

    const resendVerificationEmail = async () => {
        if(!email) {
            return;
        }
        const body: Models.ResendVerificationBody = {
            email: email
        };
        await sendPostRequest(
            `api/accounts/resend-verification-email`,
            resolveText("VerifyEmail_CouldNotSend"),
            body,
            () => {
                setVerificationEmailHasBeenSent(true);
            }
        );
    }

    if(!accountId || !verificationToken) {
        if(email) {
            return (
                <Alert variant='warning'>
                    {resolveText("VerifyEmail_NotYetVerified")} 
                    {verificationEmailHasBeenSent 
                    ? <><i className='fa fa-check green' /> {resolveText("VerifyEmail_HasBeenSent")}</>
                    : <Button onClick={resendVerificationEmail}>{resolveText("VerifyEmail_Resend")}</Button>}
                </Alert>
            );
        }
        return (
            <Alert variant='danger'>
                {resolveText("VerifyEmail_AccountOrTokenMissing")}
            </Alert>
        );
    }

    if(isSubmitting) {
        return (
            <Alert variant='info'>
                {resolveText("VerifyEmail_Verifying...")}
            </Alert>
        );
    }

    if(isVerified) {
        return (
            <Alert variant='success'>
                {resolveText("VerifyEmail_SuccessfullyVerified")}
            </Alert>
        );
    }

    return (
        <Alert variant='danger'>
            {resolveText("VerifyEmail_CouldNotVerify")} <Button onClick={verifyEmail}>{resolveText("VerifyEmail_Retry")}</Button>
        </Alert>
    );

}