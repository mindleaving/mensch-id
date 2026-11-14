import { FormEvent, useState } from 'react';
import { Alert, Button, Form, FormCheck, FormControl, FormGroup, FormLabel, FormText, InputGroup } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { showSuccessAlert } from '../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { TranslatedLinkText } from '../components/TranslatedLinkText';
import { Models } from '../types/models';

interface RegisterExternalServicePageProps {}

export const RegisterExternalServicePage = (props: RegisterExternalServicePageProps) => {

    const [ displayName, setDisplayName ] = useState<string>('');
    const [ email, setEmail ] = useState<string>('');
    const [ contactInformation, setContactInformation ] = useState<string>('');
    const [ hasAcceptedTermsAndConditions, setHasAcceptedTermsAndConditions ] = useState<boolean>(false);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ credentials, setCredentials ] = useState<Models.ExternalServices.ExternalServiceRegistrationResponse>();

    const submit = async (e?: FormEvent) => {
        e?.preventDefault();
        if(!hasAcceptedTermsAndConditions) {
            return;
        }
        const registration: Models.ExternalServices.ExternalServiceRegistration = {
            displayName: displayName,
            email: email,
            contactInformation: contactInformation
        };
        setIsSubmitting(true);
        await sendPostRequest(
            'api/externalServices', {},
            resolveText("RegisterExternalService_CouldNotRegister"),
            registration,
            async response => {
                const registrationResponse = await response.json() as Models.ExternalServices.ExternalServiceRegistrationResponse;
                setCredentials(registrationResponse);
                showSuccessAlert(resolveText("RegisterExternalService_Registered"));
            },
            undefined,
            () => setIsSubmitting(false)
        );
    };

    const copyToClipboard = async (value: string) => {
        await navigator.clipboard.writeText(value);
        showSuccessAlert(resolveText("CopiedToClipboard"));
    };

    if(credentials) {
        return (
        <>
            <h1>{resolveText("RegisterExternalService_Title")}</h1>
            <Alert variant="success">{resolveText("RegisterExternalService_CheckEmail")}</Alert>
            <Alert variant="warning">
                <strong>{resolveText("RegisterExternalService_StoreSecretWarning")}</strong>
            </Alert>
            <FormGroup className="mb-3">
                <FormLabel>client_id</FormLabel>
                <InputGroup>
                    <FormControl readOnly value={credentials.client_id} className="font-monospace" />
                    <Button variant="outline-secondary" onClick={() => copyToClipboard(credentials.client_id)}>
                        <i className="fa fa-copy" />
                    </Button>
                </InputGroup>
            </FormGroup>
            <FormGroup className="mb-3">
                <FormLabel>client_secret</FormLabel>
                <InputGroup>
                    <FormControl readOnly value={credentials.client_secret} className="font-monospace" />
                    <Button variant="outline-secondary" onClick={() => copyToClipboard(credentials.client_secret)}>
                        <i className="fa fa-copy" />
                    </Button>
                </InputGroup>
            </FormGroup>
        </>);
    }

    return (
    <>
        <h1>{resolveText("RegisterExternalService_Title")}</h1>
        <div className="lead mb-3">
            {resolveText("RegisterExternalService_Intro")}
        </div>
        <h4>{resolveText("RegisterExternalService_HowItWorks")}</h4>
        <ol>
            <li>{resolveText("RegisterExternalService_Step1")}</li>
            <li>{resolveText("RegisterExternalService_Step2")}</li>
            <li>{resolveText("RegisterExternalService_Step3")}</li>
            <li>{resolveText("RegisterExternalService_Step4")}</li>
            <li>{resolveText("RegisterExternalService_Step5")}</li>
        </ol>
        <p className="text-secondary">{resolveText("RegisterExternalService_Privacy")}</p>
        <h4>{resolveText("RegisterExternalService_GettingStarted")}</h4>
        <p>{resolveText("RegisterExternalService_LeadText")}</p>
        <Form onSubmit={submit}>
            <FormGroup className="mb-2">
                <FormLabel>{resolveText("ExternalService_DisplayName")}</FormLabel>
                <FormControl required
                    maxLength={100}
                    value={displayName}
                    onChange={e => setDisplayName(e.target.value)}
                />
                <FormText>{resolveText("RegisterExternalService_DisplayNameHint")}</FormText>
            </FormGroup>
            <FormGroup className="mb-2">
                <FormLabel>{resolveText("ExternalService_Email")}</FormLabel>
                <FormControl required
                    type="email"
                    maxLength={254}
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                />
                <FormText>{resolveText("RegisterExternalService_EmailHint")}</FormText>
            </FormGroup>
            <FormGroup className="mb-2">
                <FormLabel>{resolveText("ExternalService_ContactInformation")}</FormLabel>
                <FormControl
                    as="textarea"
                    rows={4}
                    maxLength={1000}
                    value={contactInformation}
                    onChange={e => setContactInformation(e.target.value)}
                />
                <FormText>{resolveText("RegisterExternalService_ContactInformationHint")}</FormText>
            </FormGroup>
            <FormGroup className="my-2">
                <FormCheck required
                    label={<TranslatedLinkText
                        translatedTextWithPlaceholder={resolveText("Shop_AccpetTermsAndConditions")}
                        linkTarget="/terms-of-service"
                    />}
                    checked={hasAcceptedTermsAndConditions}
                    onChange={e => setHasAcceptedTermsAndConditions(e.target.checked)}
                />
            </FormGroup>
            <AsyncButton
                type="submit"
                activeText={resolveText("Submit")}
                executingText={resolveText("Submitting...")}
                isExecuting={isSubmitting}
                disabled={!hasAcceptedTermsAndConditions}
                className="m-3"
            />
        </Form>
    </>);

};
export default RegisterExternalServicePage;
