import '../i18n';
import { act, fireEvent, render, screen } from '@testing-library/react-native';
import { ChallengeFinder } from '../components/ChallengeFinder';
import { ChallengeSearchDebounceMilliseconds } from '../helpers/ConfigurationHelpers';
import { Models } from '../types/models';

const challenge: Models.MenschIdChallenge = {
    id: 'challenge-1',
    menschId: '20000101-ACDEF',
    challengeShortId: 'ABCDEF',
    challengeSecret: 'TOPSECRET',
    createdTimestamp: '2026-11-13T10:00:00Z'
};

const advanceDebounce = async () => {
    await act(async () => {
        jest.advanceTimersByTime(ChallengeSearchDebounceMilliseconds + 10);
    });
};

describe('ChallengeFinder', () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    it('shows the scan button until at least two characters are entered', async () => {
        const searchChallenges = jest.fn().mockResolvedValue([ challenge ]);
        const onScanPressed = jest.fn();
        await render(<ChallengeFinder searchChallenges={searchChallenges} onScanPressed={onScanPressed} />);

        expect(screen.getByTestId('scan-button')).toBeTruthy();
        await fireEvent.changeText(screen.getByTestId('challenge-input'), 'a');
        expect(screen.getByTestId('scan-button')).toBeTruthy();
        await fireEvent.changeText(screen.getByTestId('challenge-input'), 'ab');
        expect(screen.queryByTestId('scan-button')).toBeNull();
        await advanceDebounce();

        expect(searchChallenges).toHaveBeenCalledWith('AB');
    });

    it('calls onScanPressed when the scan button is pressed', async () => {
        const onScanPressed = jest.fn();
        await render(<ChallengeFinder searchChallenges={jest.fn()} onScanPressed={onScanPressed} />);

        await fireEvent.press(screen.getByTestId('scan-button'));

        expect(onScanPressed).toHaveBeenCalledTimes(1);
    });

    it('shows matching challenge IDs only and reveals the secret after confirmation', async () => {
        const searchChallenges = jest.fn().mockResolvedValue([ challenge ]);
        await render(<ChallengeFinder searchChallenges={searchChallenges} onScanPressed={jest.fn()} />);

        await fireEvent.changeText(screen.getByTestId('challenge-input'), 'ab');
        await advanceDebounce();

        expect(screen.getByText('ABCDEF')).toBeTruthy();
        expect(screen.queryByText('TOPSECRET')).toBeNull();
        await fireEvent.press(screen.getByTestId('challenge-challenge-1'));
        expect(screen.getByTestId('show-secret')).toBeTruthy();
        expect(screen.queryByText('TOPSECRET')).toBeNull();
        await fireEvent.press(screen.getByTestId('show-secret'));
        expect(screen.getByText('TOPSECRET')).toBeTruthy();
    });

    it('hides the secret again when the challenge is collapsed', async () => {
        const searchChallenges = jest.fn().mockResolvedValue([ challenge ]);
        await render(<ChallengeFinder searchChallenges={searchChallenges} onScanPressed={jest.fn()} />);
        await fireEvent.changeText(screen.getByTestId('challenge-input'), 'ab');
        await advanceDebounce();
        await fireEvent.press(screen.getByTestId('challenge-challenge-1'));
        await fireEvent.press(screen.getByTestId('show-secret'));

        await fireEvent.press(screen.getByTestId('challenge-challenge-1'));
        await fireEvent.press(screen.getByTestId('challenge-challenge-1'));

        expect(screen.queryByText('TOPSECRET')).toBeNull();
        expect(screen.getByTestId('show-secret')).toBeTruthy();
    });

    it('debounces typing into a single search', async () => {
        const searchChallenges = jest.fn().mockResolvedValue([]);
        await render(<ChallengeFinder searchChallenges={searchChallenges} onScanPressed={jest.fn()} />);

        await fireEvent.changeText(screen.getByTestId('challenge-input'), 'ab');
        await fireEvent.changeText(screen.getByTestId('challenge-input'), 'abc');
        await fireEvent.changeText(screen.getByTestId('challenge-input'), 'abcd');
        await advanceDebounce();

        expect(searchChallenges).toHaveBeenCalledTimes(1);
        expect(searchChallenges).toHaveBeenCalledWith('ABCD');
        expect(screen.getByText('No matching challenges')).toBeTruthy();
    });
});
