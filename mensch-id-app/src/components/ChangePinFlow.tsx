import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { NewPinEntry } from './NewPinEntry';
import { PinEntry } from './PinEntry';
import { useAppLock } from '../lock/AppLockContext';
import { FailedPinAttemptsBeforeWarning, MaximumFailedPinAttempts } from '../lock/AppLockService';

interface ChangePinFlowProps {
    onFinished: () => void;
}

export const ChangePinFlow = (props: ChangePinFlowProps) => {

    const { t } = useTranslation();
    const appLock = useAppLock();
    const [ currentPin, setCurrentPin ] = useState<string>();
    const [ errorMessage, setErrorMessage ] = useState<string>();

    const verifyCurrentPin = async (pin: string) => {
        const result = await appLock.unlockWithPin(pin);
        if(result === 'valid') {
            setErrorMessage(undefined);
            setCurrentPin(pin);
            return;
        }
        if(result === 'invalid') {
            const failedAttempts = MaximumFailedPinAttempts - appLock.remainingPinAttempts + 1;
            const warning = failedAttempts >= FailedPinAttemptsBeforeWarning
                ? ` ${t('Pin_RemainingAttempts', { count: MaximumFailedPinAttempts - failedAttempts })}`
                : '';
            setErrorMessage(t('Pin_Wrong') + warning);
        }
    };

    const setNewPin = async (newPin: string) => {
        if(currentPin === undefined) {
            return;
        }
        const result = await appLock.changePin(currentPin, newPin);
        if(result === 'valid') {
            props.onFinished();
        }
    };

    if(currentPin === undefined) {
        return (
            <PinEntry
                title={t('Settings_ChangePin')}
                description={t('Pin_EnterCurrent')}
                errorMessage={errorMessage}
                submitTitle={t('Pin_Next')}
                onSubmit={verifyCurrentPin}
            />
        );
    }
    return (
        <NewPinEntry
            title={t('Pin_NewTitle')}
            onPinChosen={setNewPin}
        />
    );
};
