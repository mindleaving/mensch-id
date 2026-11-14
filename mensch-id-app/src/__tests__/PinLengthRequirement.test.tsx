import '../i18n';
import { fireEvent, render, screen } from '@testing-library/react-native';
import { NewPinEntry } from '../components/NewPinEntry';
import { PinEntry } from '../components/PinEntry';
import { LightColors, DarkColors } from '../styles/Theme';

const enterDigits = async (digits: string) => {
    for(const digit of digits) {
        await fireEvent.press(screen.getByTestId(`pin-key-${digit}`));
    }
};

const getHintColor = () => {
    const hintText = screen.getByText('The PIN must have at least 6 digits');
    return hintText.props.style.color;
};

const getRowHeight = () => {
    const row = screen.getByTestId('pin-length-requirement');
    return row.props.style.height;
};

describe('PIN length requirement', () => {
    it('is shown in red with a warning sign before any digit is entered', async () => {
        await render(<NewPinEntry title="Choose a PIN" onPinChosen={jest.fn()} />);

        expect(screen.getByTestId('pin-length-warning')).toBeTruthy();
        expect([ LightColors.danger, DarkColors.danger ]).toContain(getHintColor());
    });

    it('switches to the default color without icon once met, keeping the same height', async () => {
        await render(<NewPinEntry title="Choose a PIN" onPinChosen={jest.fn()} />);
        const initialHeight = getRowHeight();

        await enterDigits('12345');
        expect(screen.getByTestId('pin-length-warning')).toBeTruthy();
        await enterDigits('6');

        expect(screen.queryByTestId('pin-length-warning')).toBeNull();
        expect([ LightColors.mutedText, DarkColors.mutedText ]).toContain(getHintColor());
        expect(getRowHeight()).toBe(initialHeight);
    });

    it('is not shown when unlocking with an existing PIN', async () => {
        await render(<PinEntry title="Unlock" onSubmit={jest.fn()} />);

        expect(screen.queryByTestId('pin-length-requirement')).toBeNull();
    });
});
