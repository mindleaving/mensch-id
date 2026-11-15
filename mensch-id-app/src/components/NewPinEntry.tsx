import { ReactNode, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { PinEntry } from './PinEntry';

interface NewPinEntryProps {
    title: string;
    description?: string;
    onPinChosen: (pin: string) => Promise<void>;
    footer?: ReactNode;
}

export const NewPinEntry = (props: NewPinEntryProps) => {

    const { t } = useTranslation();
    const [ firstPin, setFirstPin ] = useState<string>();
    const [ errorMessage, setErrorMessage ] = useState<string>();

    const submitFirstPin = async (pin: string) => {
        setErrorMessage(undefined);
        setFirstPin(pin);
    };

    const submitConfirmation = async (pin: string) => {
        if(pin !== firstPin) {
            setFirstPin(undefined);
            setErrorMessage(t('Pin_Mismatch'));
            return;
        }
        await props.onPinChosen(pin);
    };

    if(firstPin === undefined) {
        return (
            <PinEntry
                key="new-pin"
                title={props.title}
                description={props.description ?? t('Pin_ChooseDescription')}
                errorMessage={errorMessage}
                submitTitle={t('Pin_Next')}
                onSubmit={submitFirstPin}
                showLengthRequirement
                footer={props.footer}
            />
        );
    }
    return (
        <PinEntry
            key="confirm-pin"
            title={t('Pin_ConfirmTitle')}
            description={t('Pin_ConfirmDescription')}
            onSubmit={submitConfirmation}
            footer={props.footer}
        />
    );
};
