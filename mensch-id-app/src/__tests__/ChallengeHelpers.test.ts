import {
    buildChallengeQrCodeContent,
    normalizeChallengeSearchText,
    parseChallengeQrCode
} from '../helpers/ChallengeHelpers';
import { isMenschId, normalizeMenschId } from '../helpers/MenschIdHelpers';

describe('parseChallengeQrCode', () => {
    it('returns the challenge ID of a valid challenge QR code', () => {
        expect(parseChallengeQrCode('menschid:challenge:0b6f3c9e-4b8a-4cde-9f1a-2a3b4c5d6e7f'))
            .toBe('0b6f3c9e-4b8a-4cde-9f1a-2a3b4c5d6e7f');
    });

    it('accepts surrounding whitespace and an upper case prefix', () => {
        expect(parseChallengeQrCode('  MENSCHID:CHALLENGE:abc123 ')).toBe('abc123');
    });

    it.each([
        [ undefined ],
        [ null ],
        [ '' ],
        [ 'menschid:challenge:' ],
        [ 'https://mensch.id/challenge/abc' ],
        [ 'menschid:person:20000101-ACDEF' ],
        [ 'menschid:challenge:../../api/admin' ],
        [ 'menschid:challenge:abc?x=1' ],
        [ `menschid:challenge:${'a'.repeat(65)}` ]
    ])('ignores invalid or foreign content %p', (content) => {
        expect(parseChallengeQrCode(content)).toBeUndefined();
    });

    it('round-trips with buildChallengeQrCodeContent', () => {
        const content = buildChallengeQrCodeContent('abc-123');

        expect(content).toBe('menschid:challenge:abc-123');
        expect(parseChallengeQrCode(content)).toBe('abc-123');
    });
});

describe('normalizeChallengeSearchText', () => {
    it('upper-cases and removes non alphanumeric characters', () => {
        expect(normalizeChallengeSearchText('ab-c d!9')).toBe('ABCD9');
    });
});

describe('MenschIdHelpers', () => {
    it.each([ '20000101-ACDEF', '20000101-acdef', ' 19991231-Z2345 ' ])('accepts %p', (value) => {
        expect(isMenschId(value)).toBe(true);
    });

    it.each([ '', '2000010-ACDEF', '20000101ACDEF', '20000101-ACDE', 'x20000101-ACDEF' ])('rejects %p', (value) => {
        expect(isMenschId(value)).toBe(false);
    });

    it('normalizes to trimmed upper case', () => {
        expect(normalizeMenschId(' 20000101-acdef ')).toBe('20000101-ACDEF');
    });
});
