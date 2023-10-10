export const isEmptyObject = (x: any) => {
    return !x || Object.keys(x).length === 0;
}