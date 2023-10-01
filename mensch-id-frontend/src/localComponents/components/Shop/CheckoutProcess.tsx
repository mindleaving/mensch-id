import { useState } from "react";
import { ContactInformationShopStep } from "./ContactInformationShopStep";
import { Models } from "../../types/models";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { ShippingMethodShopStep } from "./ShippingMethodShopStep";
import { SummaryShopStep } from "./SummaryShopStep";
import { PaymentShopStep } from "./PaymentShopStep";
import { ShoppingCartShopStep } from "./ShoppingCartShopStep";
import { useNavigate } from "react-router-dom";

interface CheckoutProcessProps {
    order: Models.Shop.Order;
    onChange: (update: Update<Models.Shop.Order>) => void;
    onPrevious?: () => void;
}

enum CheckoutStep {
    ShoppingCart = "ShoppingCart",
    ContactInformation = "ContactInformation",
    ShippingMethod = "ShippingMethod",
    PaymentMethod = "PaymentMethod",
    Summary = "Summary"
}
export const CheckoutProcess = (props: CheckoutProcessProps) => {

    const { order, onChange, onPrevious } = props;

    const [ step, setStep ] = useState<CheckoutStep>(CheckoutStep.ShoppingCart);
    const navigate = useNavigate();

    switch(step) {
        case CheckoutStep.ShoppingCart:
        default:
            return (<ShoppingCartShopStep
                order={order}
                onChange={onChange}
                onPrevious={onPrevious ?? (() => navigate("/shop"))}
                onNext={() => setStep(CheckoutStep.ContactInformation)}
            />);
        case CheckoutStep.ContactInformation:
            return (<ContactInformationShopStep
                order={order}
                onChange={onChange}
                onPrevious={() => setStep(CheckoutStep.ShoppingCart)}
                onNext={() => setStep(CheckoutStep.ShippingMethod)}
            />);
        case CheckoutStep.ShippingMethod:
            return (<ShippingMethodShopStep
                order={order}
                onChange={onChange}
                onPrevious={() => setStep(CheckoutStep.ContactInformation)}
                onNext={() => setStep(CheckoutStep.PaymentMethod)}
            />);
        case CheckoutStep.PaymentMethod:
            return (<PaymentShopStep
                order={order}
                onChange={onChange}
                onPrevious={() => setStep(CheckoutStep.ShippingMethod)}
                onNext={() => setStep(CheckoutStep.Summary)}
            />);
        case CheckoutStep.Summary:
            return (<SummaryShopStep
                order={order}
                onPrevious={() => setStep(CheckoutStep.PaymentMethod)}
            />);
    }

}