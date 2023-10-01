import { useState, FormEvent } from "react";
import { Alert, Form, FormCheck, FormControl, FormGroup, FormLabel } from "react-bootstrap";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";
import { showSuccessAlert } from "../../../sharedCommonComponents/helpers/AlertHelpers";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { sendPostRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { uuid } from "../../../sharedCommonComponents/helpers/uuid";
import { Models } from "../../types/models";

interface RequestAssignerAccountPageProps {}

export const RequestAssignerAccountPage = (props: RequestAssignerAccountPageProps) => {

    const [ name, setName ] = useState<string>('');
    const [ email, setEmail ] = useState<string>('');
    const [ expectedAssignmentsPerYear, setExpectedAssignmentsPerYear ] = useState<number>(0);
    const [ note, setNote ] = useState<string>('');
    const [ hasAcceptedTermsAndConditions, setHasAcceptedTermsAndConditions ] = useState<boolean>(false);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ hasBeenSubmitted, setHasBeenSubmitted ] = useState<boolean>(false);

    const submit = async (e?: FormEvent) => {
        e?.preventDefault();
        if(!hasAcceptedTermsAndConditions) {
            return;
        }
        const request: Models.AssignerAccountRequest = {
            id: uuid(),
            contactPersonName: name,
            email: email,
            expectedAssignmentsPerYear: expectedAssignmentsPerYear,
            note: note
        };
        setIsSubmitting(true);
        await sendPostRequest(
            'api/admin/assigner-requests', {}, 
            resolveText("Assigner_CouldNotRequest"),
            request,
            () => {
                setHasBeenSubmitted(true);
                showSuccessAlert(resolveText("Assigner_SuccessfullyRequested"));
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }
    return (
    <>
        <h1>{resolveText("Assigner_RequestAccount")}</h1>
        <div className="lead mb-2">
            {resolveText("AssignerAccountRequest_LeadText")}
        </div>
        {hasBeenSubmitted
        ? <Alert
            variant="success"
        >
            {resolveText("Assigner_SuccessfullyRequested")}
        </Alert>
        : <Form onSubmit={submit}>
            <FormGroup>
                <FormLabel>{resolveText("AssignerAccountRequest_ContactPersonName")}</FormLabel>
                <FormControl required
                    value={name}
                    onChange={e => setName(e.target.value)}
                />
            </FormGroup>
            <FormGroup>
                <FormLabel>{resolveText("AssignerAccountRequest_Email")}</FormLabel>
                <FormControl required
                    type="email"
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                />
            </FormGroup>
            <FormGroup>
                <FormLabel>{resolveText("AssignerAccountRequest_ExpectedAssignmentsPerYear")}</FormLabel>
                <FormControl required
                    type="number"
                    value={expectedAssignmentsPerYear}
                    onChange={e => setExpectedAssignmentsPerYear(Number(e.target.value))}
                    min={0}
                />
            </FormGroup>
            <FormGroup>
                <FormLabel>{resolveText("AssignerAccountRequest_Note")}</FormLabel>
                <FormControl required
                    as="textarea"
                    value={note}
                    onChange={e => setNote(e.target.value)}
                />
            </FormGroup>
            <FormGroup className="my-2">
                <FormCheck required
                    label={resolveText("Shop_AccpetTermsAndConditions")}
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
        </Form>}
    </>);

}
export default RequestAssignerAccountPage;