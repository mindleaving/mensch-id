import { RefreshTokenStorage } from '../types/frontendTypes';
import { deleteDeviceOnlyItem, loadDeviceOnlyItem, saveDeviceOnlyItem } from '../helpers/SecureStorageHelpers';

const RefreshTokenKey = 'mensch-id.refresh-token';
const IdTokenKey = 'mensch-id.id-token';

export const secureRefreshTokenStorage: RefreshTokenStorage = {
    load: async () => await loadDeviceOnlyItem(RefreshTokenKey),
    save: async (refreshToken: string) => await saveDeviceOnlyItem(RefreshTokenKey, refreshToken),
    loadIdToken: async () => await loadDeviceOnlyItem(IdTokenKey),
    saveIdToken: async (idToken: string) => await saveDeviceOnlyItem(IdTokenKey, idToken),
    clear: async () => {
        await deleteDeviceOnlyItem(RefreshTokenKey);
        await deleteDeviceOnlyItem(IdTokenKey);
    }
};
