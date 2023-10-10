import { Alert, Col, FormCheck, Row } from "react-bootstrap";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { Models } from "../../types/models";
import { PreviousNextButtonRow } from "./PreviousNextButtonRow";
import { ShippingMethod } from "../../types/enums";

interface ShippingMethodShopStepProps {
    order: Models.Shop.Order;
    onChange: (update: Update<Models.Shop.Order>) => void;
    onPrevious: () => void;
    onNext: () => void;
}

export const ShippingMethodShopStep = (props: ShippingMethodShopStepProps) => {

    const { order, onChange, onPrevious, onNext } = props;

    return (
    <>
        <h1>{resolveText("Shop")}</h1>
        <h3>{resolveText("Order_ShippingMethod")}</h3>
        {Object.values(ShippingMethod).map(shippingMethod => (
        <Alert>
            <Row>
                <Col xs="auto">
                    <FormCheck
                        type="radio"
                        checked={order.shippingMethod === shippingMethod}
                        onClick={() => onChange(state => ({
                            ...state,
                            shippingMethod: shippingMethod
                        }))}
                    />
                </Col>
                <Col>
                    {resolveText(`ShippingMethod_${shippingMethod}`)}
                </Col>
            </Row>
        </Alert>
        ))}
        <PreviousNextButtonRow
            onPrevious={onPrevious}
            onNext={onNext}
            canMoveNext={!!order.shippingMethod}
        />
    </>);

}