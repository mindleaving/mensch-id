export const validateSearchParam = (searchParam: string | null, validValues: string[], defaultValue: string) => {
    if(!searchParam) {
        return defaultValue;
    }
    return validValues.map(str => str.toLowerCase()).includes(searchParam.toLowerCase()) 
        ? searchParam 
        : defaultValue;
}