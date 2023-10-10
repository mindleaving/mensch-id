import { Alert, Col, FormCheck, Row } from "react-bootstrap";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { PaymentMethod } from "../../types/enums";
import { Models } from "../../types/models";
import { PreviousNextButtonRow } from "./PreviousNextButtonRow";

interface PaymentShopStepProps {
    order: Models.Shop.Order;
    onChange: (update: Update<Models.Shop.Order>) => void;
    onPrevious: () => void;
    onNext: () => void;
}

export const PaymentShopStep = (props: PaymentShopStepProps) => {

    const { order, onChange, onPrevious, onNext } = props;

    return (
    <>
        <h1>{resolveText("Shop")}</h1>
        <h3>{resolveText("Order_PaymentMethod")}</h3>
        {Object.values(PaymentMethod).map(paymentMethod => (
            <Alert
                key={paymentMethod}
            >
                <Row>
                    <Col xs="auto">
                        <FormCheck
                            type="radio"
                            checked={order.paymentMethod === paymentMethod}
                            onClick={() => onChange(state => ({
                                ...state,
                                paymentMethod: paymentMethod
                            }))}
                        />
                    </Col>
                    <Col>
                        {resolveText(`PaymentMethod_${paymentMethod}`)}
                    </Col>
                </Row>
            </Alert>
        ))}
        <PreviousNextButtonRow
            onPrevious={onPrevious}
            onNext={onNext}
            canMoveNext={!!order.paymentMethod}
        />
    </>);

}