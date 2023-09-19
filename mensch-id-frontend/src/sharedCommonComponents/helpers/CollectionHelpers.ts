import { compareAsc, compareDesc } from "date-fns";
import { Groups } from "../types/frontendTypes";

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
export const groupBy = <T extends unknown>(collection: T[], keySelector: (item: T) => string): Groups<T> => {
    const groups = groupIntoDictionary(collection, keySelector);
    return Object.keys(groups).map(key => ({
        key: key,
        items: groups[key]
    }));
}
export const sum = <T extends unknown>(collection: T[], valueSelector: (item: T) => number) => {
    let sum = 0;
    for (const item of collection) {
        const value = valueSelector(item);
        sum += value;
    }
    return sum;
}
export const average = <T extends unknown>(collection: T[], valueSelector: (item: T) => number) => {
    if(collection.length === 0) {
        return 0;
    }
    return sum(collection, valueSelector) / collection.length;
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
export const areEquivalent = <T extends unknown>(collection1: T[] | undefined, collection2: T[] | undefined) => {
    if(!collection1 && !collection2) {
        return true;
    }
    if(!collection1 || !collection2) {
        return false;
    }
    if(collection1.length !== collection2.length) {
        return false;
    }
    return collection1.every(item1 => collection2.includes(item1));
}
export const firstOrDefault = <T extends unknown>(items: T[]): T | undefined => {
    if(!items || items.length === 0) {
        return undefined;
    }
    return items[0];
}
export const last = <T extends unknown>(collection: T[]): T | undefined => {
    if(!collection || collection.length === 0) {
        return undefined;
    }
    return collection[collection.length-1];
}
export const range = (start: number, end: number) => {
    if(end < start) {
        return [];
    }
    return Array.from({ length: end - start + 1}, (_,i) => start + i);
}
export const sortByTimeAscending = <T extends unknown>(entries: T[], timeFieldSelector: (item: T) => Date): T[] => {
    return [...entries].sort((a,b) => compareAsc(timeFieldSelector(a), timeFieldSelector(b)));
}
export const sortByTimeDescending = <T extends unknown>(entries: T[], timeFieldSelector: (item: T) => Date): T[] => {
    return [...entries].sort((a,b) => compareDesc(timeFieldSelector(a), timeFieldSelector(b)));
}
export const intersect = <T extends unknown>(collection1: T[], collection2: T[], itemComparer: (item1: T, item2: T) => boolean) => {
    if(!collection1 || !collection2) {
        return [];
    }
    return collection1.filter(item1 => collection2.some(item2 => itemComparer(item1,item2)));
}
export const moveItem = (collection: any[], from: number, to: number) => {
    if(from < 0 || from >= collection.length) {
        throw new Error("Invalid index of item to be reordered");
    }
    if(to < 0 || to >= collection.length) {
        throw new Error("Invalid target index for item to be reordered");
    }
    if(to === from) {
        return collection;
    }
    const itemToBeMoved = collection[from];
    if(from < to) {
        return [ 
            ...collection.slice(0, from),
            ...collection.slice(from + 1, to + 1),
            itemToBeMoved,
            ...collection.slice(to + 1)
        ];
    } else {
        return [ 
            ...collection.slice(0, to),
            itemToBeMoved,
            ...collection.slice(to, from),
            ...collection.slice(from+1)
        ];
    }
}