import '../i18n';
import { fireEvent, render, screen } from '@testing-library/react-native';
import { PinEntry } from '../components/PinEntry';
import { NewPinEntry } from '../components/NewPinEntry';

const enterDigits = async (digits: string) => {
    for(const digit of digits) {
        await fireEvent.press(screen.getByTestId(`pin-key-${digit}`));
    }
};

describe('PinEntry', () => {
    it('enables submission only from 6 digits on and submits the entered PIN', async () => {
        const onSubmit = jest.fn().mockResolvedValue(undefined);
        await render(<PinEntry title="Unlock" onSubmit={onSubmit} />);

        await enterDigits('12345');
        await fireEvent.press(screen.getByTestId('pin-submit'));
        expect(onSubmit).not.toHaveBeenCalled();
        await enterDigits('6');
        await fireEvent.press(screen.getByTestId('pin-submit'));

        expect(onSubmit).toHaveBeenCalledWith('123456');
    });

    it('deletes the last digit', async () => {
        const onSubmit = jest.fn().mockResolvedValue(undefined);
        await render(<PinEntry title="Unlock" onSubmit={onSubmit} />);

        await enterDigits('1234567');
        await fireEvent.press(screen.getByTestId('pin-key-delete'));
        await fireEvent.press(screen.getByTestId('pin-submit'));

        expect(onSubmit).toHaveBeenCalledWith('123456');
    });

    it('shows the extra key only when an action is provided', async () => {
        const onExtraKeyPressed = jest.fn();
        await render(<PinEntry title="Unlock" onSubmit={jest.fn()} extraKeyIcon="fingerprint" onExtraKeyPressed={onExtraKeyPressed} />);

        await fireEvent.press(screen.getByTestId('pin-key-extra'));

        expect(onExtraKeyPressed).toHaveBeenCalledTimes(1);
    });
});

describe('NewPinEntry', () => {
    it('requires the PIN to be entered twice', async () => {
        const onPinChosen = jest.fn().mockResolvedValue(undefined);
        await render(<NewPinEntry title="Choose a PIN" onPinChosen={onPinChosen} />);

        await enterDigits('246810');
        await fireEvent.press(screen.getByTestId('pin-submit'));
        await enterDigits('246810');
        await fireEvent.press(screen.getByTestId('pin-submit'));

        expect(onPinChosen).toHaveBeenCalledWith('246810');
    });

    it('starts over when the confirmation does not match', async () => {
        const onPinChosen = jest.fn().mockResolvedValue(undefined);
        await render(<NewPinEntry title="Choose a PIN" onPinChosen={onPinChosen} />);

        await enterDigits('246810');
        await fireEvent.press(screen.getByTestId('pin-submit'));
        await enterDigits('111111');
        await fireEvent.press(screen.getByTestId('pin-submit'));

        expect(onPinChosen).not.toHaveBeenCalled();
        expect(screen.getByTestId('pin-error')).toBeTruthy();
        expect(screen.getByText('Choose a PIN')).toBeTruthy();
    });
});
