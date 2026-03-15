import { getMonth, getDate, getYear, isSameDay, isLeapYear, format } from "date-fns";

export const isBirthday = (date: Date, birthDate: Date) => {
    const isSameDateAndMonth = getMonth(date) === getMonth(birthDate) && getDate(date) === getDate(birthDate);
    if(isSameDateAndMonth) {
        return true;
    }
    // Check for birthday on 29th of February which only exists in leap years
    // If current year is not a leap year, report birthday on 1st of March
    // Beware JS quirk: months are zero-indexed -> 0 = January, 1 = February,...
    const is29thOfFebruary = getMonth(birthDate) === 1 && getDate(birthDate) === 29;
    if(is29thOfFebruary && !isLeapYear(date)) {
        return isSameDay(date, new Date(getYear(date), 2, 1));
    }
    return false;
}
export const toDateOnly = (date: Date): string => {
    return format(date, 'yyyy-MM-dd');
}
export const toTimeOnly = (date: Date): string => {
    return format(date, 'HH:mm:ss');
}
export const isValidDate = (date: Date) => {
    return !isNaN(date.getTime());
}