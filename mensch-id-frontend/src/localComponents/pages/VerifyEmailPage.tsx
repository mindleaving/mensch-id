import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { Alert, Button } from 'react-bootstrap';
import { ResendVerificationEmailForm } from './ResendVerificationEmailForm';
import { NotificationManager } from 'react-notifications';
import { HorizontalLineWithText } from '../../sharedCommonComponents/components/HorizontalLineWithText';

interface VerifyEmailPageProps {}

export const VerifyEmailPage = (props: VerifyEmailPageProps) => {

    const [ q ] = useSearchParams();
    const accountId = q.get("accountId");
    const verificationToken = q.get("token");
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(true);
    const [ isVerified, setIsVerified ] = useState<boolean>(false);
    const navigate = useNavigate();

    const verifyEmail = useMemo(() => async () => {
        if(!accountId || !verificationToken) {
            return;
        }
        setIsSubmitting(true);
        try {
            await apiClient.instance!.get(`api/accounts/${accountId}/verify-email`, { token: verificationToken });
            setIsVerified(true);
            setTimeout(() => navigate("/login"), 3000);
            NotificationManager.info(resolveText("Redirecting..."));
        } catch {
            setIsVerified(false);
        } finally {
            setIsSubmitting(false);
        }
    }, [ accountId, verificationToken]);

    useEffect(() => {  
        verifyEmail();
    }, [ verifyEmail ]);

    

    if(!accountId || !verificationToken) {
        return (
        <>
            <h3>{resolveText("VerifyEmail_NotYetVerified")}</h3>
            <Alert variant='danger'>
                {resolveText("VerifyEmail_AccountOrTokenMissing")}
            </Alert>
            <ResendVerificationEmailForm />
        </>
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
                <Button 
                    onClick={() => navigate("/login")}
                    className="mx-3"
                >
                    {resolveText("Login")}
                </Button>
            </Alert>
        );
    }

    return (
        <>
            <Alert variant='danger'>
                {resolveText("VerifyEmail_CouldNotVerify")}
                <Button 
                    onClick={verifyEmail}
                    className='mx-3'
                >
                    {resolveText("VerifyEmail_Retry")}
                </Button>
            </Alert>
            <HorizontalLineWithText text={resolveText("or")} />
            <h5>{resolveText("VerifyEmail_Resend")}</h5>
            <ResendVerificationEmailForm />
        </>
    );

}