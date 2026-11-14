import * as SecureStore from 'expo-secure-store';

const DeviceOnlyOptions: SecureStore.SecureStoreOptions = {
    keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY
};

export const loadDeviceOnlyItem = async (key: string): Promise<string | null> => {
    return await SecureStore.getItemAsync(key, DeviceOnlyOptions);
};

export const saveDeviceOnlyItem = async (
    key: string,
    value: string
): Promise<void> => {
    await SecureStore.deleteItemAsync(key, DeviceOnlyOptions);
    await SecureStore.setItemAsync(key, value, DeviceOnlyOptions);
};

export const deleteDeviceOnlyItem = async (key: string): Promise<void> => {
    await SecureStore.deleteItemAsync(key, DeviceOnlyOptions);
};
