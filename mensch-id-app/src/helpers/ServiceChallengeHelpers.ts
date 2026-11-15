export const ServiceChallengeLinkPrefix = 'menschid://service-challenge/';
export const ServiceChallengeRoute = '/service-challenge';

const ServiceChallengeIdPattern = /^[A-HJ-NP-Z2-9]{52}$/;
const ServiceChallengePathPattern = /^\/*service-challenge\/([^/?#]+)\/?$/i;

export const isValidServiceChallengeId = (challengeId: string | null | undefined): boolean => {
    return !!challengeId && ServiceChallengeIdPattern.test(challengeId);
};

export const parseServiceChallengeLink = (link: string | null | undefined): string | undefined => {
    if(!link) {
        return undefined;
    }
    const trimmedLink = link.trim();
    if(!trimmedLink.toLowerCase().startsWith(ServiceChallengeLinkPrefix)) {
        return undefined;
    }
    const challengeId = trimmedLink.substring(ServiceChallengeLinkPrefix.length).replace(/\/+$/, '');
    return isValidServiceChallengeId(challengeId) ? challengeId : undefined;
};

export const parseServiceChallengePath = (path: string): string | undefined => {
    const withoutQuery = path.split(/[?#]/)[0];
    const match = ServiceChallengePathPattern.exec(withoutQuery);
    if(!match) {
        return undefined;
    }
    return isValidServiceChallengeId(match[1]) ? match[1] : undefined;
};

export const buildServiceChallengeRoute = (challengeId: string): string => {
    return `${ServiceChallengeRoute}?id=${challengeId}`;
};

export const getRemainingSeconds = (expiresAt: string, now: number): number => {
    const expiryTime = Date.parse(expiresAt);
    if(Number.isNaN(expiryTime)) {
        return 0;
    }
    return Math.max(0, Math.floor((expiryTime - now) / 1000));
};

export const formatRemainingTime = (remainingSeconds: number): string => {
    const minutes = Math.floor(remainingSeconds / 60);
    const seconds = remainingSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
};
