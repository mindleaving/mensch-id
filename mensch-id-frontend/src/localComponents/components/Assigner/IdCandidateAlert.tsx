import { useState } from "react";
import { Alert, Row, Col } from "react-bootstrap";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";
import { CopyButton } from "../../../sharedCommonComponents/components/CopyButton";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { sendPostRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";

interface IdCandidateAlertProps {
    idCandidate: string;
    onAccepted: (menschId: string) => void;
    onRejected: () => void;
}
export const IdCandidateAlert = (props: IdCandidateAlertProps) => {

    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    const acceptId = async () => {
        setIsSubmitting(true);
        await sendPostRequest(
            `api/assigner/claim/${props.idCandidate}`,
            resolveText("Assigner_CouldNotClaimId"),
            null,
            () => {
                props.onAccepted(props.idCandidate);
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    const rejectId = async () => {
        setIsSubmitting(true);
        await sendPostRequest(
            `api/assigner/reject/${props.idCandidate}`,
            resolveText("Assigner_CouldNotRejectId"),
            null,
            () => {
                props.onRejected()
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    return (
        <Alert
            variant="success"
        >
            <Row className='align-items-center'>
                <Col xs={3}>{resolveText("Person_ID")}</Col>
                <Col xs={4} lg={2}>
                    <strong className='text-nowrap'>{props.idCandidate}</strong>
                </Col>
                <Col>
                    <CopyButton
                        value={props.idCandidate}
                        size='sm'
                    />
                </Col>
            </Row>
            <Row className='mt-2'>
                <Col>
                    <AsyncButton
                        variant='success'
                        className='me-2'
                        onClick={acceptId}
                        activeText={resolveText("IdCandidate_Accept")}
                        executingText={resolveText("IdCandidate_Accepting")}
                        isExecuting={isSubmitting}
                    />
                    <AsyncButton
                        variant='danger'
                        className='me-2'
                        onClick={rejectId}
                        activeText={resolveText("IdCandidate_Reject")}
                        executingText={resolveText("IdCandidate_Rejecting")}
                        isExecuting={isSubmitting}
                    />
                </Col>
            </Row>
        </Alert>
    );
}