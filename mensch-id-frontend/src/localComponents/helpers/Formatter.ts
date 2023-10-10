import { format } from "date-fns";
import { Models } from "../types/models";

export const formatMoney = (money: Models.Shop.Money) => {
    return `${money.currency} ${money.value.toFixed(2)}`;
}
export const formatDateTime = (date: Date) => {
    return format(date, 'yyyy-MM-dd HH:mm');
}
export const formatDate = (date: Date) => {
    return format(date, 'yyyy-MM-dd');
}
export const formatTime = (date: Date) => {
    return format(date, 'HH:mm');
}