export const toDictionary = <T extends unknown>(items: T[], keySelector: (item: T) => string) => {
    return Object.assign({}, ...items.map(item => ({ [keySelector(item)]: item})));
}