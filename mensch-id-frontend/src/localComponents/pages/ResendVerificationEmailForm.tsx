import React, { FormEvent, useState } from 'react';
import { Form, FormGroup, FormLabel, FormControl, Button, Alert, Row, Col } from 'react-bootstrap';
import { useSearchParams } from 'react-router-dom';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../types/models';

interface ResendVerificationEmailFormProps {}

export const ResendVerificationEmailForm = (props: ResendVerificationEmailFormProps) => {

    const [ q ] = useSearchParams();
    const [ email, setEmail ] = useState<string>(q.get("email") ?? '');
    const [ verificationEmailHasBeenSent, setVerificationEmailHasBeenSent ] = useState<boolean>(false);
    
    const resendVerificationEmail = async (e?: FormEvent) => {
        e?.preventDefault();
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

    if(verificationEmailHasBeenSent) {
        return (
            <Alert variant='success'>
                <Row>
                    <Col xs="auto">
                        <i className='fa fa-check green' />
                    </Col>
                    <Col>
                        {resolveText("VerifyEmail_HasBeenSent")}
                    </Col>
                </Row>
            </Alert>
        )
    }

    return (
        <Form onSubmit={resendVerificationEmail}>
            <FormGroup>
                <FormLabel>{resolveText("Email")}</FormLabel>
                <FormControl
                    value={email}
                    onChange={(e:any) => setEmail(e.target.value)}
                />
            </FormGroup>
            <Button
                type="submit"
                className='m-3'
                disabled={!email.includes("@")}
            >
                {resolveText("VerifyEmail_Resend")}
            </Button>
        </Form>
    );

}