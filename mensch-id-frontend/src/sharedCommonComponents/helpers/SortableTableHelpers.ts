import { compareAsc } from "date-fns";

export const compareBoolean = (a: boolean, b: boolean) => {
    return (a ? 1 : 0) - (b ? 1 : 0);
}
export const compareString = (a: string | undefined, b: string | undefined) => {
    if(!a && !b) {
        return 0;
    }
    if(!a) {
        return 1;
    }
    if(!b) {
        return -1;
    }
    return a.localeCompare(b);
}
export const compareDateTime = (a: Date | undefined, b: Date | undefined) => {
    if(!a && !b) {
        return 0;
    }
    if(!a) {
        return 1;
    }
    if(!b) {
        return -1;
    }
    return compareAsc(a, b);
}