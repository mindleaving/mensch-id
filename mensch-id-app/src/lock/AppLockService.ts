import { AppLockDependencies, AppLockSettings, PinVerificationResult } from '../types/frontendTypes';

export const MinimumPinLength = 6;
export const MaximumPinLength = 12;
export const BiometricUsesBeforePinRequired = 10;
export const MinimumHoursBetweenPinRequirements = 12;
export const MaximumFailedPinAttempts = 10;
export const FailedPinAttemptsBeforeWarning = 5;

export const LockSuppressionGraceMilliseconds = 1500;

const MillisecondsPerHour = 60 * 60 * 1000;

export class InvalidPinFormatError extends Error {
    constructor() {
        super(`The PIN must consist of ${MinimumPinLength} to ${MaximumPinLength} digits`);
        this.name = 'InvalidPinFormatError';
    }
}

export const isValidPinFormat = (pin: string): boolean => {
    return new RegExp(`^[0-9]{${MinimumPinLength},${MaximumPinLength}}$`).test(pin);
};

export const isLockSuppressed = (
    activeSuppressionCount: number,
    lastSuppressionEndTimestamp: number,
    now: number
): boolean => {
    if(activeSuppressionCount > 0) {
        return true;
    }
    return now - lastSuppressionEndTimestamp < LockSuppressionGraceMilliseconds;
};

export const isPinRequiredAfterBiometrics = (
    biometricUseCount: number,
    lastPinEntryTimestamp: number | undefined,
    now: number
): boolean => {
    if(lastPinEntryTimestamp === undefined) {
        return true;
    }
    if(biometricUseCount < BiometricUsesBeforePinRequired) {
        return false;
    }
    return now - lastPinEntryTimestamp >= MinimumHoursBetweenPinRequirements * MillisecondsPerHour;
};

export class AppLockService {
    constructor(private readonly dependencies: AppLockDependencies) {}

    hasPin = async (): Promise<boolean> => {
        const settings = await this.dependencies.storage.load();
        return settings.pinHash !== undefined && settings.pinSalt !== undefined;
    };

    isBiometricsEnabled = async (): Promise<boolean> => {
        const settings = await this.dependencies.storage.load();
        return settings.isBiometricsEnabled;
    };

    getRemainingPinAttempts = async (): Promise<number> => {
        const settings = await this.dependencies.storage.load();
        return MaximumFailedPinAttempts - settings.failedPinAttempts;
    };

    setPin = async (pin: string): Promise<void> => {
        if(!isValidPinFormat(pin)) {
            throw new InvalidPinFormatError();
        }
        const settings = await this.dependencies.storage.load();
        const pinSalt = await this.dependencies.createSalt();
        const pinHash = await this.dependencies.hashPin(pin, pinSalt);
        await this.dependencies.storage.save({
            ...settings,
            pinHash: pinHash,
            pinSalt: pinSalt,
            failedPinAttempts: 0,
            biometricUseCount: 0,
            lastPinEntryTimestamp: this.dependencies.now()
        });
    };

    changePin = async (currentPin: string, newPin: string): Promise<PinVerificationResult> => {
        const verificationResult = await this.verifyPin(currentPin);
        if(verificationResult === 'valid') {
            await this.setPin(newPin);
        }
        return verificationResult;
    };

    verifyPin = async (pin: string): Promise<PinVerificationResult> => {
        const settings = await this.dependencies.storage.load();
        if(settings.pinHash === undefined || settings.pinSalt === undefined) {
            return 'invalid';
        }
        const pinHash = await this.dependencies.hashPin(pin, settings.pinSalt);
        if(pinHash === settings.pinHash) {
            await this.dependencies.storage.save({
                ...settings,
                failedPinAttempts: 0,
                biometricUseCount: 0,
                lastPinEntryTimestamp: this.dependencies.now()
            });
            return 'valid';
        }
        const failedPinAttempts = settings.failedPinAttempts + 1;
        if(failedPinAttempts >= MaximumFailedPinAttempts) {
            await this.reset();
            return 'lockedOut';
        }
        await this.dependencies.storage.save({ ...settings, failedPinAttempts: failedPinAttempts });
        return 'invalid';
    };

    recordBiometricUnlock = async (): Promise<boolean> => {
        const settings = await this.dependencies.storage.load();
        const biometricUseCount = settings.biometricUseCount + 1;
        await this.dependencies.storage.save({ ...settings, biometricUseCount: biometricUseCount });
        return isPinRequiredAfterBiometrics(biometricUseCount, settings.lastPinEntryTimestamp, this.dependencies.now());
    };

    setBiometricsEnabled = async (isEnabled: boolean): Promise<void> => {
        const settings = await this.dependencies.storage.load();
        await this.dependencies.storage.save({ ...settings, isBiometricsEnabled: isEnabled, biometricUseCount: 0 });
    };

    reset = async (): Promise<void> => {
        const emptySettings: AppLockSettings = {
            isBiometricsEnabled: false,
            failedPinAttempts: 0,
            biometricUseCount: 0
        };
        await this.dependencies.storage.save(emptySettings);
    };
}
