import { formatMenschIdInput } from '../helpers/MenschIdHelpers';

describe('formatMenschIdInput', () => {
    it('does not insert a dash before the date part is complete', () => {
        expect(formatMenschIdInput('1990010', '199001')).toBe('1990010');
    });

    it('inserts the dash after the eighth character', () => {
        expect(formatMenschIdInput('19900101', '1990010')).toBe('19900101-');
        expect(formatMenschIdInput('19900101-a', '19900101-')).toBe('19900101-A');
    });

    it('ignores a dash typed by the user after the automatic one', () => {
        expect(formatMenschIdInput('19900101--', '19900101-')).toBe('19900101-');
        expect(formatMenschIdInput('19900101-AB-C', '19900101-AB')).toBe('19900101-ABC');
    });

    it('allows deleting the automatically inserted dash', () => {
        expect(formatMenschIdInput('19900101', '19900101-')).toBe('19900101');
        expect(formatMenschIdInput('1990010', '19900101')).toBe('1990010');
    });

    it('formats pasted values with or without dash', () => {
        expect(formatMenschIdInput('19900101acdef', '')).toBe('19900101-ACDEF');
        expect(formatMenschIdInput('19900101-ACDEF', '')).toBe('19900101-ACDEF');
    });
});
