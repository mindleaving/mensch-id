import { AuthService } from '../auth/AuthService';
import { RefreshTokenStorage, TokenEndpoints } from '../types/frontendTypes';

export const createMemoryStorage = (
    initialRefreshToken: string | null = null
): RefreshTokenStorage & { value: string | null; idToken: string | null } => {
    const storage = {
        value: initialRefreshToken,
        idToken: null as string | null,
        load: async () => storage.value,
        save: async (refreshToken: string) => {
            storage.value = refreshToken;
        },
        loadIdToken: async () => storage.idToken,
        saveIdToken: async (idToken: string) => {
            storage.idToken = idToken;
        },
        clear: async () => {
            storage.value = null;
            storage.idToken = null;
        }
    };
    return storage;
};

export const createAuthService = (
    storage: RefreshTokenStorage,
    tokenEndpoints: TokenEndpoints
): AuthService => {
    const authService = new AuthService(storage);
    authService.configure(tokenEndpoints);
    return authService;
};

export const createJsonResponse = (status: number, body?: unknown): Response => {
    return {
        ok: status >= 200 && status < 300,
        status: status,
        statusText: `Status ${status}`,
        json: async () => body,
        text: async () => (body === undefined ? '' : JSON.stringify(body))
    } as Response;
};
