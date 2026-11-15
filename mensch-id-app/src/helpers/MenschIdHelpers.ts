export const isMenschId = (value: string): boolean => /^[0-9]{8}-[A-Z0-9]{5}$/i.test(value.trim());

export const normalizeMenschId = (value: string): string => value.trim().toUpperCase();

const MenschIdDatePartLength = 8;

export const formatMenschIdInput = (input: string, previousValue: string): string => {
    const characters = input.toUpperCase().replace(/-/g, '');
    const isDeleting = input.length < previousValue.length;
    const isDatePartIncomplete = characters.length < MenschIdDatePartLength;
    if(isDatePartIncomplete || (characters.length === MenschIdDatePartLength && isDeleting)) {
        return characters;
    }
    return `${characters.substring(0, MenschIdDatePartLength)}-${characters.substring(MenschIdDatePartLength)}`;
};
