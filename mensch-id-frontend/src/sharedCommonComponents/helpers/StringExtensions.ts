export const firstLetterToUpper = (str: string) => {
    if(!str) {
        return str;
    }
    return str[0].toUpperCase() + str.substring(1);
}
export const parseEnum = <T extends string>(enumType: { [key:string]: string }, str: string) => {
    return Object.entries(enumType).find(kvp => kvp[0].toLowerCase() === str.toLowerCase())?.[1] as T;
}
export const removeSurroundingQuotes = (str: string | null | undefined) => {
    if(!str) {
        return str;
    }
    if((str.startsWith('\'') && str.endsWith('\''))
        || (str.startsWith('"') && str.endsWith('"'))) {
        return str.substring(1, str.length-1);
    }
    return str;
}
export const isNullOrEmpty = (word: string) => {
    return word === undefined || word === null || word === '';
}