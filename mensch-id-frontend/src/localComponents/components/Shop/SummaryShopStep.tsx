import { Card, Col, FormCheck, Row } from "react-bootstrap";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Models } from "../../types/models";
import { PreviousNextButtonRow } from "./PreviousNextButtonRow";
import { useState, FormEvent } from "react";
import { showSuccessAlert } from "../../../sharedCommonComponents/helpers/AlertHelpers";
import { sendPostRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";

interface SummaryShopStepProps {
    order: Models.Shop.Order;
    onPrevious: () => void;
}

export const SummaryShopStep = (props: SummaryShopStepProps) => {

    const { order, onPrevious } = props;

    const [ hasAcceptedTermsAndConditions, setHasAcceptedTermsAndConditions ] = useState<boolean>(false);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    const submit = async (e?: FormEvent) => {
        e?.preventDefault();
        if(order.items.length === 0) {
            return;
        }
        if(!hasAcceptedTermsAndConditions) {
            return;
        }
        setIsSubmitting(true);
        await sendPostRequest(
            'api/shop/submit-order', {},
            resolveText("Shop_CouldNotOrder"),
            order,
            () => {
                showSuccessAlert(resolveText("Order_SuccessfullyPlaced"));
                // TODO: Reset order
                setHasAcceptedTermsAndConditions(false);
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    return (
    <>
        <h1>{resolveText("Shop")}</h1>
        <h3>{resolveText("Order_Summary")}</h3>
        <Card className="my-2">
            <Card.Header>
                <Card.Title>{resolveText("Order_Items")}</Card.Title>
            </Card.Header>
            <Card.Body>

            </Card.Body>
        </Card>
        <Card className="my-2">
            <Card.Header>
                <Card.Title>{resolveText("ContactInformation")}</Card.Title>
            </Card.Header>
            <Card.Body>
                <Row>
                    <Col lg>

                    </Col>
                    <Col lg>

                    </Col>
                </Row>
            </Card.Body>
        </Card>
        <FormCheck
            label={resolveText("Shop_AccpetTermsAndConditions")}
            checked={hasAcceptedTermsAndConditions}
            onChange={e => setHasAcceptedTermsAndConditions(e.target.checked)}
        />
        <PreviousNextButtonRow
            onPrevious={onPrevious}
            onNext={submit}
            canMoveNext={hasAcceptedTermsAndConditions}
        />
        <AsyncButton
            onClick={submit}
            isExecuting={isSubmitting}
            activeText={resolveText("Submit")}
            executingText={resolveText("Submitting...")}
            disabled={order.items.length === 0 || !hasAcceptedTermsAndConditions}
        />
    </>);

}