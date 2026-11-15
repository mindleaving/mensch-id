import {
    formatRemainingTime,
    getRemainingSeconds,
    parseServiceChallengeLink,
    parseServiceChallengePath
} from '../helpers/ServiceChallengeHelpers';
import { resolveSystemPath } from '../helpers/LinkingHelpers';
import { ServiceChallengeState } from '../types/enums';
import { ViewModels } from '../types/viewModels';

const ValidChallengeId = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789ABCDEFGHJKLMNPQRSTUV';

const createChallenge = (id: string, expiresAt: string): ViewModels.ServiceChallengeViewModel => ({
    id: id,
    serviceDisplayName: 'Service',
    serviceContactInformation: 'contact@example.org',
    description: 'Sign in',
    state: ServiceChallengeState.Pending,
    createdTimestamp: '2026-11-14T10:00:00Z',
    expiresAt: expiresAt
});

describe('ServiceChallengeHelpers', () => {
    it('parses valid service challenge links', () => {
        expect(parseServiceChallengeLink(`menschid://service-challenge/${ValidChallengeId}`)).toBe(ValidChallengeId);
        expect(parseServiceChallengeLink(` menschid://service-challenge/${ValidChallengeId}/ `)).toBe(ValidChallengeId);
    });

    it('rejects invalid service challenge links', () => {
        expect(parseServiceChallengeLink(undefined)).toBeUndefined();
        expect(parseServiceChallengeLink('menschid://service-challenge/ABC')).toBeUndefined();
        expect(parseServiceChallengeLink(`menschid://service-challenge/${ValidChallengeId.toLowerCase()}`)).toBeUndefined();
        expect(parseServiceChallengeLink(`https://evil.example.org/service-challenge/${ValidChallengeId}`)).toBeUndefined();
        expect(parseServiceChallengeLink(`menschid:challenge:${ValidChallengeId}`)).toBeUndefined();
    });

    it('parses service challenge paths', () => {
        expect(parseServiceChallengePath(`/service-challenge/${ValidChallengeId}`)).toBe(ValidChallengeId);
        expect(parseServiceChallengePath(`service-challenge/${ValidChallengeId}?x=1`)).toBe(ValidChallengeId);
        expect(parseServiceChallengePath('/service-challenge/INVALID')).toBeUndefined();
    });

    it('calculates and formats remaining time', () => {
        const now = Date.parse('2026-11-14T10:00:00Z');
        expect(getRemainingSeconds('2026-11-14T10:01:05Z', now)).toBe(65);
        expect(getRemainingSeconds('2026-11-14T09:59:00Z', now)).toBe(0);
        expect(formatRemainingTime(65)).toBe('1:05');
    });

});

describe('resolveSystemPath for service challenges', () => {
    it('routes service challenge links to the service challenge screen', () => {
        expect(resolveSystemPath(`menschid://service-challenge/${ValidChallengeId}`, true)).toBe(`/service-challenge?id=${ValidChallengeId}`);
        expect(resolveSystemPath(`/service-challenge/${ValidChallengeId}`, false)).toBe(`/service-challenge?id=${ValidChallengeId}`);
    });

    it('leaves invalid service challenge paths untouched', () => {
        expect(resolveSystemPath('/service-challenge/INVALID', false)).toBe('/service-challenge/INVALID');
    });
});
