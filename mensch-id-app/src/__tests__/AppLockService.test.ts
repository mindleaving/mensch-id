import {
    AppLockService,
    BiometricUsesBeforePinRequired,
    InvalidPinFormatError,
    isPinRequiredAfterBiometrics,
    isValidPinFormat,
    MaximumFailedPinAttempts
} from '../lock/AppLockService';
import { AppLockDependencies, AppLockSettings } from '../types/frontendTypes';

const Hour = 60 * 60 * 1000;

const createService = () => {
    let settings: AppLockSettings = { isBiometricsEnabled: false, failedPinAttempts: 0, biometricUseCount: 0 };
    let now = 1_000_000_000_000;
    let saltCounter = 0;
    const dependencies: AppLockDependencies = {
        storage: {
            load: async () => ({ ...settings }),
            save: async (newSettings: AppLockSettings) => {
                settings = { ...newSettings };
            }
        },
        hashPin: async (pin: string, salt: string) => `hash(${salt}:${pin})`,
        createSalt: async () => `salt${++saltCounter}`,
        now: () => now
    };
    return {
        service: new AppLockService(dependencies),
        getSettings: () => settings,
        advanceTime: (milliseconds: number) => {
            now += milliseconds;
        }
    };
};

describe('isValidPinFormat', () => {
    it.each([ '123456', '000000', '123456789012' ])('accepts %p', (pin) => {
        expect(isValidPinFormat(pin)).toBe(true);
    });

    it.each([ '', '12345', '1234567890123', '12345a', '12 3456', '١٢٣٤٥٦' ])('rejects %p', (pin) => {
        expect(isValidPinFormat(pin)).toBe(false);
    });
});

describe('isPinRequiredAfterBiometrics', () => {
    const now = 100 * Hour;

    it('does not require the PIN before the 10th biometric use', () => {
        expect(isPinRequiredAfterBiometrics(BiometricUsesBeforePinRequired - 1, now - 48 * Hour, now)).toBe(false);
    });

    it('requires the PIN on the 10th biometric use if the last PIN entry is at least 12 hours ago', () => {
        expect(isPinRequiredAfterBiometrics(BiometricUsesBeforePinRequired, now - 12 * Hour, now)).toBe(true);
    });

    it('does not require the PIN on the 10th use if the PIN was entered within the last 12 hours', () => {
        expect(isPinRequiredAfterBiometrics(BiometricUsesBeforePinRequired, now - 11 * Hour, now)).toBe(false);
    });

    it('requires the PIN on later uses once 12 hours have passed', () => {
        expect(isPinRequiredAfterBiometrics(BiometricUsesBeforePinRequired + 5, now - 13 * Hour, now)).toBe(true);
    });

    it('requires the PIN if it was never entered', () => {
        expect(isPinRequiredAfterBiometrics(1, undefined, now)).toBe(true);
    });
});

describe('AppLockService', () => {
    it('stores only a salted hash of the PIN', async () => {
        const { service, getSettings } = createService();

        await service.setPin('123456');

        expect(await service.hasPin()).toBe(true);
        expect(getSettings().pinHash).toBe('hash(salt1:123456)');
        expect(JSON.stringify(getSettings())).not.toContain('"123456"');
    });

    it('rejects PINs that are too short or not numeric', async () => {
        const { service } = createService();

        await expect(service.setPin('12345')).rejects.toBeInstanceOf(InvalidPinFormatError);
        await expect(service.setPin('abcdef')).rejects.toBeInstanceOf(InvalidPinFormatError);
        expect(await service.hasPin()).toBe(false);
    });

    it('verifies the correct PIN and rejects a wrong one', async () => {
        const { service } = createService();
        await service.setPin('123456');

        expect(await service.verifyPin('654321')).toBe('invalid');
        expect(await service.getRemainingPinAttempts()).toBe(MaximumFailedPinAttempts - 1);
        expect(await service.verifyPin('123456')).toBe('valid');
        expect(await service.getRemainingPinAttempts()).toBe(MaximumFailedPinAttempts);
    });

    it('locks out and resets after too many wrong PINs', async () => {
        const { service } = createService();
        await service.setPin('123456');
        await service.setBiometricsEnabled(true);

        const results = [];
        for(let attempt = 0; attempt < MaximumFailedPinAttempts; attempt++) {
            results.push(await service.verifyPin('000000'));
        }

        expect(results.slice(0, -1).every(result => result === 'invalid')).toBe(true);
        expect(results[results.length - 1]).toBe('lockedOut');
        expect(await service.hasPin()).toBe(false);
        expect(await service.isBiometricsEnabled()).toBe(false);
    });

    it('changes the PIN only with the correct current PIN', async () => {
        const { service } = createService();
        await service.setPin('123456');

        expect(await service.changePin('000000', '222222')).toBe('invalid');
        expect(await service.verifyPin('123456')).toBe('valid');
        expect(await service.changePin('123456', '222222')).toBe('valid');
        expect(await service.verifyPin('222222')).toBe('valid');
        expect(await service.verifyPin('123456')).toBe('invalid');
    });

    it('requires the PIN on the 10th biometric unlock once 12 hours passed since the last PIN entry', async () => {
        const { service, advanceTime } = createService();
        await service.setPin('123456');
        await service.setBiometricsEnabled(true);
        advanceTime(13 * Hour);

        const pinRequirements = [];
        for(let use = 1; use <= BiometricUsesBeforePinRequired; use++) {
            pinRequirements.push(await service.recordBiometricUnlock());
        }

        expect(pinRequirements.slice(0, -1).every(isRequired => !isRequired)).toBe(true);
        expect(pinRequirements[pinRequirements.length - 1]).toBe(true);
    });

    it('does not require the PIN on the 10th biometric unlock within 12 hours and resets after PIN entry', async () => {
        const { service, advanceTime } = createService();
        await service.setPin('123456');
        await service.setBiometricsEnabled(true);
        advanceTime(Hour);
        for(let use = 1; use < BiometricUsesBeforePinRequired; use++) {
            await service.recordBiometricUnlock();
        }

        const isRequiredWithinTwelveHours = await service.recordBiometricUnlock();
        advanceTime(12 * Hour);
        const isRequiredAfterTwelveHours = await service.recordBiometricUnlock();
        await service.verifyPin('123456');
        const isRequiredAfterPinEntry = await service.recordBiometricUnlock();

        expect(isRequiredWithinTwelveHours).toBe(false);
        expect(isRequiredAfterTwelveHours).toBe(true);
        expect(isRequiredAfterPinEntry).toBe(false);
    });

    it('reset removes PIN and biometrics setting', async () => {
        const { service } = createService();
        await service.setPin('123456');
        await service.setBiometricsEnabled(true);

        await service.reset();

        expect(await service.hasPin()).toBe(false);
        expect(await service.isBiometricsEnabled()).toBe(false);
    });
});
