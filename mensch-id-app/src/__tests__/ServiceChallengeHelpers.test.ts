import {
    addOrReplaceServiceChallenge,
    formatRemainingTime,
    getActiveServiceChallenges,
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

    it('adds new challenges first and replaces existing ones', () => {
        const first = createChallenge('A', '2026-11-14T10:05:00Z');
        const second = createChallenge('B', '2026-11-14T10:05:00Z');
        const updatedFirst = { ...first, description: 'Updated' };

        const challenges = addOrReplaceServiceChallenge(addOrReplaceServiceChallenge([], first), second);
        const updatedChallenges = addOrReplaceServiceChallenge(challenges, updatedFirst);

        expect(challenges.map(x => x.id)).toEqual([ 'B', 'A' ]);
        expect(updatedChallenges.map(x => x.description)).toEqual([ 'Updated', 'Sign in' ]);
    });

    it('filters expired challenges', () => {
        const now = Date.parse('2026-11-14T10:00:00Z');
        const active = createChallenge('A', '2026-11-14T10:05:00Z');
        const expired = createChallenge('B', '2026-11-14T09:55:00Z');

        expect(getActiveServiceChallenges([ active, expired ], now)).toEqual([ active ]);
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
