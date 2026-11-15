import { InvalidRefreshTokenError, SessionExpiredError } from '../auth/AuthService';
import { createAuthService, createMemoryStorage } from '../testing/TestHelpers';

describe('AuthService', () => {
    it('stores the refresh token and keeps the access token in memory when a session starts', async () => {
        const storage = createMemoryStorage();
        const authService = createAuthService(storage, {
            refreshTokens: jest.fn(),
            revokeRefreshToken: jest.fn()
        });

        await authService.startSession({ accessToken: 'access-1', refreshToken: 'refresh-1' });

        expect(authService.getAccessToken()).toBe('access-1');
        expect(storage.value).toBe('refresh-1');
    });

    it('restores a session with the stored refresh token and rotates it', async () => {
        const storage = createMemoryStorage('refresh-1');
        const refreshTokens = jest.fn().mockResolvedValue({ accessToken: 'access-2', refreshToken: 'refresh-2' });
        const authService = createAuthService(storage, { refreshTokens, revokeRefreshToken: jest.fn() });

        const isRestored = await authService.restoreSession();

        expect(isRestored).toBe(true);
        expect(refreshTokens).toHaveBeenCalledWith('refresh-1');
        expect(authService.getAccessToken()).toBe('access-2');
        expect(storage.value).toBe('refresh-2');
    });

    it('does not restore a session without a stored refresh token', async () => {
        const refreshTokens = jest.fn();
        const authService = createAuthService(createMemoryStorage(), { refreshTokens, revokeRefreshToken: jest.fn() });

        const isRestored = await authService.restoreSession();

        expect(isRestored).toBe(false);
        expect(refreshTokens).not.toHaveBeenCalled();
    });

    it('runs concurrent refreshes only once', async () => {
        let resolveRefresh: (value: { accessToken: string; refreshToken: string }) => void = () => undefined;
        const refreshTokens = jest.fn().mockImplementation(() => new Promise(resolve => {
            resolveRefresh = resolve;
        }));
        const authService = createAuthService(createMemoryStorage('refresh-1'), { refreshTokens, revokeRefreshToken: jest.fn() });

        const firstRefresh = authService.refreshAccessToken();
        const secondRefresh = authService.refreshAccessToken();
        await Promise.resolve();
        await Promise.resolve();
        resolveRefresh({ accessToken: 'access-2', refreshToken: 'refresh-2' });

        await expect(firstRefresh).resolves.toBe('access-2');
        await expect(secondRefresh).resolves.toBe('access-2');
        expect(refreshTokens).toHaveBeenCalledTimes(1);
    });

    it('ends the session when the refresh token is rejected', async () => {
        const storage = createMemoryStorage('refresh-1');
        const refreshTokens = jest.fn().mockRejectedValue(new InvalidRefreshTokenError());
        const authService = createAuthService(storage, { refreshTokens, revokeRefreshToken: jest.fn() });
        const onSessionEnded = jest.fn();
        authService.onSessionEnded(onSessionEnded);

        await expect(authService.refreshAccessToken()).rejects.toBeInstanceOf(SessionExpiredError);

        expect(storage.value).toBeNull();
        expect(authService.getAccessToken()).toBeUndefined();
        expect(onSessionEnded).toHaveBeenCalledTimes(1);
    });

    it('keeps the refresh token when refreshing fails for a transient reason', async () => {
        const storage = createMemoryStorage('refresh-1');
        const refreshTokens = jest.fn().mockRejectedValue(new TypeError('Network request failed'));
        const authService = createAuthService(storage, { refreshTokens, revokeRefreshToken: jest.fn() });
        const onSessionEnded = jest.fn();
        authService.onSessionEnded(onSessionEnded);

        await expect(authService.restoreSession()).rejects.toBeInstanceOf(TypeError);

        expect(storage.value).toBe('refresh-1');
        expect(onSessionEnded).not.toHaveBeenCalled();
    });

    it('revokes the refresh token and clears the session on logout', async () => {
        const storage = createMemoryStorage();
        const revokeRefreshToken = jest.fn().mockResolvedValue(undefined);
        const authService = createAuthService(storage, { refreshTokens: jest.fn(), revokeRefreshToken });
        await authService.startSession({ accessToken: 'access-1', refreshToken: 'refresh-1' });

        await authService.endSession();

        expect(revokeRefreshToken).toHaveBeenCalledWith('refresh-1');
        expect(storage.value).toBeNull();
        expect(authService.getAccessToken()).toBeUndefined();
    });

    it('clears the local session even if revocation fails', async () => {
        const storage = createMemoryStorage();
        const revokeRefreshToken = jest.fn().mockRejectedValue(new Error('offline'));
        const authService = createAuthService(storage, { refreshTokens: jest.fn(), revokeRefreshToken });
        await authService.startSession({ accessToken: 'access-1', refreshToken: 'refresh-1' });

        await authService.endSession();

        expect(storage.value).toBeNull();
        expect(authService.hasAccessToken()).toBe(false);
    });
    it('stores the id token and returns it when the session ends', async () => {
        const storage = createMemoryStorage();
        const refreshTokens = jest.fn().mockResolvedValue({ accessToken: 'access-2', refreshToken: 'refresh-2', idToken: 'id-2' });
        const authService = createAuthService(storage, { refreshTokens, revokeRefreshToken: jest.fn().mockResolvedValue(undefined) });
        await authService.startSession({ accessToken: 'access-1', refreshToken: 'refresh-1', idToken: 'id-1' });
        expect(storage.idToken).toBe('id-1');

        await authService.refreshAccessToken();
        const idToken = await authService.endSession();

        expect(idToken).toBe('id-2');
        expect(storage.idToken).toBeNull();
        expect(storage.value).toBeNull();
    });
});
