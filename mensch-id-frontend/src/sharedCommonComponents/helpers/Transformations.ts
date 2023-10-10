export const toDictionary = <T extends unknown, TOut extends unknown>(
    items: T[], 
    keySelector: (item: T) => string, 
    itemSelector?: (item: T) => TOut)
    : { [key:string]: TOut } => {
    return Object.assign(
        {}, 
        ...items.map(item => ({ 
            [keySelector(item)]: itemSelector ? itemSelector(item) : item
        }))
    );
}