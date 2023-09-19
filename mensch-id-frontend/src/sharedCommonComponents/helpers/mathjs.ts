import { create, all } from 'mathjs';

export const mathjs = create(all, { });
mathjs.createUnit('M', {
    definition: "1 mol/L",
    prefixes: "short"
});

export const canConvertTo = (value: number, sourceUnit: string, targetUnit: string) => {
    if(sourceUnit === targetUnit) {
        return true;
    }
    try {
        mathjs.unit(value, sourceUnit).to(targetUnit);
        return true;
    } catch {
        return false;
    }
}