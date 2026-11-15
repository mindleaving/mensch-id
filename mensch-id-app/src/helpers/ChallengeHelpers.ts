export const ChallengeQrCodePrefix = 'menschid:challenge:';

const ChallengeIdPattern = /^[A-Za-z0-9-]{1,64}$/;

export const parseChallengeQrCode = (qrCodeContent: string | null | undefined): string | undefined => {
    if(!qrCodeContent) {
        return undefined;
    }
    const trimmedContent = qrCodeContent.trim();
    if(!trimmedContent.toLowerCase().startsWith(ChallengeQrCodePrefix)) {
        return undefined;
    }
    const challengeId = trimmedContent.substring(ChallengeQrCodePrefix.length);
    if(!ChallengeIdPattern.test(challengeId)) {
        return undefined;
    }
    return challengeId;
};

export const buildChallengeQrCodeContent = (challengeId: string): string => {
    return `${ChallengeQrCodePrefix}${challengeId}`;
};

export const normalizeChallengeSearchText = (searchText: string): string => {
    return searchText.toUpperCase().replace(/[^A-Z0-9]/g, '');
};
