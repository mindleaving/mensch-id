import { useEffect, useState } from 'react';
import { Alert } from 'react-bootstrap';
import { useSearchParams } from 'react-router-dom';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

type VerificationState = 'verifying' | 'verified' | 'failed' | 'missingParameters';

interface VerifyExternalServiceEmailPageProps {}

export const VerifyExternalServiceEmailPage = (props: VerifyExternalServiceEmailPageProps) => {

    const [ q ] = useSearchParams();
    const serviceId = q.get("serviceId");
    const verificationToken = q.get("token");
    const hasParameters = !!serviceId && !!verificationToken;
    const [ state, setState ] = useState<VerificationState>(hasParameters ? 'verifying' : 'missingParameters');

    useEffect(() => {
        if(!serviceId || !verificationToken) {
            return;
        }
        let isCancelled = false;
        const verify = async () => {
            try {
                await apiClient.instance!.get(`api/externalServices/${serviceId}/verify-email`, { token: verificationToken });
                if(!isCancelled) {
                    setState('verified');
                }
            } catch {
                if(!isCancelled) {
                    setState('failed');
                }
            }
        };
        verify();
        return () => {
            isCancelled = true;
        };
    }, [ serviceId, verificationToken ]);

    return (
    <>
        <h1>{resolveText("VerifyServiceEmail_Title")}</h1>
        {state === 'verifying'
        ? <Alert variant="info">{resolveText("VerifyEmail_Verifying...")}</Alert>
        : state === 'verified'
        ? <Alert variant="success">{resolveText("VerifyServiceEmail_Success")}</Alert>
        : state === 'failed'
        ? <Alert variant="danger">{resolveText("VerifyServiceEmail_Failed")}</Alert>
        : <Alert variant="danger">{resolveText("VerifyServiceEmail_MissingParameters")}</Alert>}
    </>);

};
export default VerifyExternalServiceEmailPage;
