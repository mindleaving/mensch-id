import { Models } from "../types/models";

export const formatMoney = (money: Models.Shop.Money) => {
    return `${money.currency} ${money.value.toFixed(2)}`;
}