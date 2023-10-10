export const formatOwnershipSecret = (str: string) => {
    const parts = removeInvalidCharactersFromOwnershipSecret(str).match(/.{1,6}/g);
    if(parts === null) {
        return '';
    }
    return parts!.join("  ");
}
export const removeInvalidCharactersFromOwnershipSecret = (str: string) => {
    return str.toUpperCase().replaceAll(/[^A-Z0-9]/g,"");
}