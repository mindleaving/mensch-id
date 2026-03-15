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
export const truncateText = (str: string, maxLength: number) => {
    if(!str) {
        return str;
    }
    if(str.length <= maxLength) {
        return str;
    }
    return str.substring(0, maxLength - 3) + '...';
}
export const includesAll = (str: string, searchTerms: string[]) => {
    for (const searchTerm of searchTerms) {
        if(!str.includes(searchTerm)) {
            return false;
        }
    }
    return true;
}
export const insertTextAt = (str: string, insertText: string, position: number) => {
    return str.slice(0, position) + insertText + str.slice(position);
}