import '../i18n';
import { fireEvent, render, screen } from '@testing-library/react-native';
import { CreatedChallengeView } from '../components/CreatedChallengeView';
import { DarkColors, LightColors } from '../styles/Theme';
import { Models } from '../types/models';

const challenge: Models.MenschIdChallenge = {
    id: 'challenge-1',
    menschId: '20000101-ACDEF',
    challengeShortId: 'QWERTY',
    challengeSecret: 'XK4P7M',
    createdTimestamp: '2026-11-14T10:00:00Z'
};

describe('CreatedChallengeView', () => {
    it('first shows only the challenge ID with its instruction', async () => {
        await render(<CreatedChallengeView challenge={challenge} />);

        expect(screen.getByTestId('created-challenge-id')).toHaveTextContent('QWERTY');
        expect(screen.getByText('Done! Next...')).toBeTruthy();
        expect(screen.queryByText('XK4P7M')).toBeNull();
        expect(screen.queryByTestId('show-created-secret')).toBeNull();
    });

    it('then replaces the content with the small ID, secret instructions and a reveal button', async () => {
        await render(<CreatedChallengeView challenge={challenge} />);

        await fireEvent.press(screen.getByTestId('challenge-id-done'));

        expect(screen.queryByTestId('challenge-step-id')).toBeNull();
        expect(screen.queryByTestId('created-challenge-id')).toBeNull();
        expect(screen.getByTestId('created-challenge-id-small')).toHaveTextContent('Challenge ID: QWERTY');
        expect(screen.getByTestId('show-created-secret')).toBeTruthy();
        expect(screen.queryByText('XK4P7M')).toBeNull();
    });

    it('reveals the secret in red', async () => {
        await render(<CreatedChallengeView challenge={challenge} />);
        await fireEvent.press(screen.getByTestId('challenge-id-done'));

        await fireEvent.press(screen.getByTestId('show-created-secret'));

        const secret = screen.getByTestId('created-challenge-secret');
        expect(secret).toHaveTextContent('XK4P7M');
        expect(screen.queryByTestId('show-created-secret')).toBeNull();
        expect([ LightColors.danger, DarkColors.danger ]).toContain(secret.props.style.find((style: { color?: string }) => style?.color)?.color);
    });
});
