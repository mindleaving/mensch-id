import * as SecureStore from 'expo-secure-store';
import { RefreshTokenStorage } from '../types/frontendTypes';

const RefreshTokenKey = 'mensch-id.refresh-token';

export const secureRefreshTokenStorage: RefreshTokenStorage = {
    load: async () => await SecureStore.getItemAsync(RefreshTokenKey),
    save: async (refreshToken: string) => await SecureStore.setItemAsync(RefreshTokenKey, refreshToken),
    clear: async () => await SecureStore.deleteItemAsync(RefreshTokenKey)
};
