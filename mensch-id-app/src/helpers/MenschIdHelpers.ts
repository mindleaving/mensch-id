export const isMenschId = (value: string): boolean => /^[0-9]{8}-[A-Z0-9]{5}$/i.test(value.trim());

export const normalizeMenschId = (value: string): string => value.trim().toUpperCase();
