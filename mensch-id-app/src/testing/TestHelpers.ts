import { AuthService } from '../auth/AuthService';
import { RefreshTokenStorage, TokenEndpoints } from '../types/frontendTypes';

export const createMemoryStorage = (initialRefreshToken: string | null = null): RefreshTokenStorage & { value: string | null } => {
    const storage = {
        value: initialRefreshToken,
        load: async () => storage.value,
        save: async (refreshToken: string) => {
            storage.value = refreshToken;
        },
        clear: async () => {
            storage.value = null;
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
