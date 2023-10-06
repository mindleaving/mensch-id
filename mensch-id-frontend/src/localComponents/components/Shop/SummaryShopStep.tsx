import { Card, Col, FormCheck, Row } from "react-bootstrap";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Models } from "../../types/models";
import { PreviousNextButtonRow } from "./PreviousNextButtonRow";
import { useState, FormEvent } from "react";
import { showSuccessAlert } from "../../../sharedCommonComponents/helpers/AlertHelpers";
import { sendPostRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";
import { ContactInformationViewer } from "./ContactInformationViewer";
import { ShoppingCartTable } from "./ShoppingCartTable";
import { TranslatedLinkText } from "../TranslatedLinkText";

interface SummaryShopStepProps {
    order: Models.Shop.Order;
    onPrevious: () => void;
    onOrderSubmitted: () => void;
}

export const SummaryShopStep = (props: SummaryShopStepProps) => {

    const { order, onPrevious, onOrderSubmitted } = props;

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
                onOrderSubmitted();
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
                <ShoppingCartTable
                    isReadOnly
                    order={order}
                    onChange={_ => {}}
                />
            </Card.Body>
        </Card>
        <Card className="my-2">
            <Card.Header>
                <Card.Title>{resolveText("ContactInformation")}</Card.Title>
            </Card.Header>
            <Card.Body>
                <Row className="g-3">
                    <Col lg>
                        <div className="mb-3">
                            <h5>{resolveText("Order_PaymentMethod")}: {resolveText(`PaymentMethod_${order.paymentMethod}`)}</h5>
                        </div>

                        <strong>{resolveText("Order_InvoiceAddress")}</strong>
                        <ContactInformationViewer
                            contactInformation={order.invoiceAddress}
                        />
                    </Col>
                    <Col lg>
                        <div className="mb-3">
                            <h5>{resolveText("Order_ShippingMethod")}: {resolveText(`ShippingMethod_${order.shippingMethod}`)}</h5>
                        </div>

                        <strong>{resolveText("Order_ShippingAddress")}</strong>
                        <ContactInformationViewer
                            contactInformation={order.shippingAddress}
                        />
                    </Col>
                </Row>
            </Card.Body>
        </Card>
        <FormCheck
            label={<TranslatedLinkText
                translatedTextWithPlaceholder={resolveText("Shop_AccpetTermsAndConditions")}
                linkTarget={"/terms-of-service"}
            />}
            checked={hasAcceptedTermsAndConditions}
            onChange={e => setHasAcceptedTermsAndConditions(e.target.checked)}
        />
        <PreviousNextButtonRow
            onPrevious={onPrevious}
            onNext={submit}
            canMoveNext={hasAcceptedTermsAndConditions}
            nextButton={
                <AsyncButton
                    onClick={submit}
                    isExecuting={isSubmitting}
                    activeText={resolveText("Order_Submit")}
                    executingText={resolveText("Submitting...")}
                    disabled={order.items.length === 0 || !hasAcceptedTermsAndConditions}
                />
            }
        />
    </>);

}