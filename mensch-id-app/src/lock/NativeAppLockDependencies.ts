import * as Crypto from 'expo-crypto';
import * as LocalAuthentication from 'expo-local-authentication';
import { loadDeviceOnlyItem, saveDeviceOnlyItem } from '../helpers/SecureStorageHelpers';
import { AppLockDependencies, AppLockSettings } from '../types/frontendTypes';

const AppLockSettingsKey = 'mensch-id.app-lock';
const PinHashIterations = 200;

const DefaultSettings: AppLockSettings = {
    isBiometricsEnabled: false,
    failedPinAttempts: 0,
    biometricUseCount: 0
};

const toHex = (bytes: Uint8Array): string => {
    return Array.from(bytes, byte => byte.toString(16).padStart(2, '0')).join('');
};

const hashPin = async (pin: string, salt: string): Promise<string> => {
    let hash = `${salt}:${pin}`;
    for(let iteration = 0; iteration < PinHashIterations; iteration++) {
        hash = await Crypto.digestStringAsync(Crypto.CryptoDigestAlgorithm.SHA256, `${salt}:${hash}`);
    }
    return hash;
};

export const nativeAppLockDependencies: AppLockDependencies = {
    storage: {
        load: async () => {
            const json = await loadDeviceOnlyItem(AppLockSettingsKey);
            if(!json) {
                return { ...DefaultSettings };
            }
            return { ...DefaultSettings, ...JSON.parse(json) as Partial<AppLockSettings> };
        },
        save: async (settings: AppLockSettings) => {
            await saveDeviceOnlyItem(AppLockSettingsKey, JSON.stringify(settings));
        }
    },
    hashPin: hashPin,
    createSalt: async () => toHex(Crypto.getRandomBytes(16)),
    now: () => Date.now()
};

export const isBiometricsAvailable = async (): Promise<boolean> => {
    const hasHardware = await LocalAuthentication.hasHardwareAsync();
    if(!hasHardware) {
        return false;
    }
    return await LocalAuthentication.isEnrolledAsync();
};

export const authenticateWithBiometrics = async (
    promptMessage: string,
    cancelLabel: string
): Promise<boolean> => {
    const result = await LocalAuthentication.authenticateAsync({
        promptMessage: promptMessage,
        cancelLabel: cancelLabel,
        disableDeviceFallback: true,
        biometricsSecurityLevel: 'strong'
    });
    return result.success;
};
