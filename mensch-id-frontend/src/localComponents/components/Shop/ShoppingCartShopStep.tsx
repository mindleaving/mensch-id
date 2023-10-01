import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { Models } from "../../types/models";
import { PreviousNextButtonRow } from "./PreviousNextButtonRow";

interface ShoppingCartShopStepProps {
    order: Models.Shop.Order;
    onChange: (update: Update<Models.Shop.Order>) => void;
    onPrevious: () => void;
    onNext: () => void;
}

export const ShoppingCartShopStep = (props: ShoppingCartShopStepProps) => {

    const { order, onChange, onPrevious, onNext } = props;

    return (
    <>
        <h1>{resolveText("Shop")}</h1>
        <h3>{resolveText("Shop_ShoppingCart")}</h3>

        <PreviousNextButtonRow
            onPrevious={onPrevious}
            onNext={onNext}
            canMoveNext={order.items.length > 0}
        />
    </>);

}