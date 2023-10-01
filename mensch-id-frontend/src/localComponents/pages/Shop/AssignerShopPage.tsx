import { useState, useContext } from "react";
import { Models } from "../../types/models";
import { uuid } from "../../../sharedCommonComponents/helpers/uuid";
import { OrderStatus, PaymentMethod, ShippingMethod } from "../../types/enums.d";
import UserContext from "../../contexts/UserContext";
import { ViewModels } from "../../types/viewModels";
import { ProductSelectionShopStep } from "../../components/Shop/ProductSelectionShopStep";
import { CheckoutProcess } from "../../components/Shop/CheckoutProcess";

interface AssignerShopPageProps {}

enum ShopSteps {
    ProductSelection = "ProductSelection",
    Checkout = "Checkout"
}
export const AssignerShopPage = (props: AssignerShopPageProps) => {

    const user = useContext(UserContext) as ViewModels.AssignerAccountViewModel;
    const [ step, setStep ] = useState<ShopSteps>(ShopSteps.ProductSelection);
    const [ order, setOrder ] = useState<Models.Shop.Order>({
        id: uuid(),
        items: [],
        status: OrderStatus.Placed,
        orderedByAccountId: user.accountId,
        invoiceAddress: user.contactInformation,
        sendInvoiceSeparately: false,
        shippingAddress: user.contactInformation,
        shippingMethod: ShippingMethod.Standard,
        paymentMethod: PaymentMethod.Invoice,
        statusChanges: []
    });

    const reset = () => {
        setOrder({
            id: uuid(),
            items: [],
            status: OrderStatus.Placed,
            orderedByAccountId: user.accountId,
            invoiceAddress: user.contactInformation,
            sendInvoiceSeparately: false,
            shippingAddress: user.contactInformation,
            shippingMethod: ShippingMethod.Standard,
            paymentMethod: PaymentMethod.Invoice,
            statusChanges: []
        });
        setStep(ShopSteps.ProductSelection);
    }

    switch(step) {
        case ShopSteps.ProductSelection:
        default:
            return (<ProductSelectionShopStep
                order={order}
                onChange={setOrder}
                goToCheckout={() => setStep(ShopSteps.Checkout)}
            />);
        case ShopSteps.Checkout:
            return (<CheckoutProcess
                order={order}
                onChange={setOrder}
                onPrevious={() => setStep(ShopSteps.ProductSelection)}
                onOrderSubmitted={reset}
            />);
    }

}
export default AssignerShopPage;