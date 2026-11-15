export const ChallengeQrCodePrefix = 'menschid:challenge:';

export const buildChallengeQrCodeContent = (challengeId: string): string => {
    return `${ChallengeQrCodePrefix}${challengeId}`;
};
