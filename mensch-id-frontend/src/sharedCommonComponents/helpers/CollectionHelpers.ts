export const groupIntoDictionary = <T extends unknown>(collection: T[], keySelector: (item: T) => string) => {
    const groups: { [k: string]: T[] } = {};
    for (const item of collection) {
        const key = keySelector(item);
        if(!groups[key]) {
            groups[key] = [];
        };
        groups[key].push(item);
    }
    return groups;
}
export const groupBy = <T extends unknown>(collection: T[], keySelector: (item: T) => string) => {
    const groups = groupIntoDictionary(collection, keySelector);
    return Object.keys(groups).map(key => ({
        key: key,
        items: groups[key]
    }));
}
export const mostCommonValue = <T extends string | number>(collection: T[]): T | undefined => {
    const distinctItems: { [key: string | number]: number } = {};
    let maxCount  = 0;
    let maxCountItem: T | undefined = undefined;
    for (const item of collection) {
        if(distinctItems[item] == null) {
            distinctItems[item] = 0;
        }
        const currentCount = distinctItems[item]
        distinctItems[item] = currentCount + 1;
        if(currentCount+1 > maxCount) {
            maxCount = currentCount+1;
            maxCountItem = item;
        }
    }
    return maxCountItem;
}
export const distinct = <T extends unknown>(collection: T[]): T[] => {
    // Credit: https://stackoverflow.com/questions/1960473/get-all-unique-values-in-a-javascript-array-remove-duplicates
    return collection.filter((item, index, self) => self.indexOf(item) === index);
}
export const areEqual = <T extends unknown>(collection1: T[], collection2: T[]): boolean => {
    if(collection1.length !== collection2.length) {
        return false;
    }
    for (let index = 0; index < collection1.length; index++) {
        const element1 = collection1[index];
        const element2 = collection2[index];
        if(element1 !== element2) {
            return false;
        }
    }
    return true;
}
export const last = <T extends unknown>(collection: T[]): T | undefined => {
    if(!collection || collection.length === 0) {
        return undefined;
    }
    return collection[collection.length-1];
}