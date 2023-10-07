import { Currency, ShippingMethod } from "../types/enums";
import { Models } from "../types/models";

export const getShipmentCost = (shippingMethod: ShippingMethod): Models.Shop.Money => {
    switch(shippingMethod) {
        case ShippingMethod.Standard:
        default:
            return {
                currency: Currency.EUR,
                value: 6.99
            };
    }
}