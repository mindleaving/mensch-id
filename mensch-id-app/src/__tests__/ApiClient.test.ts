import { ApiClient, ApiError } from '../communication/ApiClient';
import { InvalidRefreshTokenError, SessionExpiredError } from '../auth/AuthService';
import { createAuthService, createJsonResponse, createMemoryStorage } from '../testing/TestHelpers';

const BaseUrl = 'https://api.example.org';

const getAuthorizationHeader = (fetchMock: jest.Mock, callIndex: number): string | undefined => {
    const init = fetchMock.mock.calls[callIndex][1] as RequestInit;
    return (init.headers as Record<string, string>).Authorization;
};

describe('ApiClient', () => {
    it('sends the access token as bearer token', async () => {
        const authService = createAuthService(createMemoryStorage(), { refreshTokens: jest.fn(), revokeRefreshToken: jest.fn() });
        await authService.startSession({ accessToken: 'access-1', refreshToken: 'refresh-1' });
        const fetchMock = jest.fn().mockResolvedValue(createJsonResponse(200, { id: '20000101-ACDEF' }));
        const apiClient = new ApiClient(BaseUrl, authService, fetchMock);

        const result = await apiClient.get<{ id: string }>('api/profiles/me');

        expect(result.id).toBe('20000101-ACDEF');
        expect(fetchMock.mock.calls[0][0]).toBe(`${BaseUrl}/api/profiles/me`);
        expect(getAuthorizationHeader(fetchMock, 0)).toBe('Bearer access-1');
    });

    it('refreshes the access token once and retries after a 401 response', async () => {
        const refreshTokens = jest.fn().mockResolvedValue({ accessToken: 'access-2', refreshToken: 'refresh-2' });
        const authService = createAuthService(createMemoryStorage(), { refreshTokens, revokeRefreshToken: jest.fn() });
        await authService.startSession({ accessToken: 'expired', refreshToken: 'refresh-1' });
        const fetchMock = jest.fn()
            .mockResolvedValueOnce(createJsonResponse(401))
            .mockResolvedValueOnce(createJsonResponse(200, []));
        const apiClient = new ApiClient(BaseUrl, authService, fetchMock);

        const result = await apiClient.get<unknown[]>('api/challenges?searchText=AB');

        expect(result).toEqual([]);
        expect(refreshTokens).toHaveBeenCalledTimes(1);
        expect(getAuthorizationHeader(fetchMock, 0)).toBe('Bearer expired');
        expect(getAuthorizationHeader(fetchMock, 1)).toBe('Bearer access-2');
    });

    it('refreshes only once for parallel requests that all receive 401', async () => {
        const refreshTokens = jest.fn().mockResolvedValue({ accessToken: 'access-2', refreshToken: 'refresh-2' });
        const authService = createAuthService(createMemoryStorage(), { refreshTokens, revokeRefreshToken: jest.fn() });
        await authService.startSession({ accessToken: 'expired', refreshToken: 'refresh-1' });
        const fetchMock = jest.fn().mockImplementation(async (_url: string, init: RequestInit) => {
            const authorization = (init.headers as Record<string, string>).Authorization;
            return authorization === 'Bearer access-2' ? createJsonResponse(200, {}) : createJsonResponse(401);
        });
        const apiClient = new ApiClient(BaseUrl, authService, fetchMock);

        await Promise.all([ apiClient.get('api/a'), apiClient.get('api/b'), apiClient.get('api/c') ]);

        expect(refreshTokens).toHaveBeenCalledTimes(1);
    });

    it('fails with SessionExpiredError when the refresh token is rejected', async () => {
        const refreshTokens = jest.fn().mockRejectedValue(new InvalidRefreshTokenError());
        const authService = createAuthService(createMemoryStorage(), { refreshTokens, revokeRefreshToken: jest.fn() });
        await authService.startSession({ accessToken: 'expired', refreshToken: 'refresh-1' });
        const onSessionEnded = jest.fn();
        authService.onSessionEnded(onSessionEnded);
        const fetchMock = jest.fn().mockResolvedValue(createJsonResponse(401));
        const apiClient = new ApiClient(BaseUrl, authService, fetchMock);

        await expect(apiClient.get('api/profiles/me')).rejects.toBeInstanceOf(SessionExpiredError);

        expect(fetchMock).toHaveBeenCalledTimes(1);
        expect(onSessionEnded).toHaveBeenCalledTimes(1);
    });

    it('throws ApiError with status code for unsuccessful responses', async () => {
        const authService = createAuthService(createMemoryStorage(), { refreshTokens: jest.fn(), revokeRefreshToken: jest.fn() });
        await authService.startSession({ accessToken: 'access-1', refreshToken: 'refresh-1' });
        const fetchMock = jest.fn().mockResolvedValue(createJsonResponse(404, 'Not found'));
        const apiClient = new ApiClient(BaseUrl, authService, fetchMock);

        const request = apiClient.get('api/challenges/unknown');

        await expect(request).rejects.toBeInstanceOf(ApiError);
        await expect(request).rejects.toMatchObject({ statusCode: 404 });
    });

    it('sends JSON bodies for POST requests', async () => {
        const authService = createAuthService(createMemoryStorage(), { refreshTokens: jest.fn(), revokeRefreshToken: jest.fn() });
        await authService.startSession({ accessToken: 'access-1', refreshToken: 'refresh-1' });
        const fetchMock = jest.fn().mockResolvedValue(createJsonResponse(200, { id: 'challenge-1' }));
        const apiClient = new ApiClient(BaseUrl, authService, fetchMock);

        await apiClient.post('api/id/20000101-ACDEF/challenge', { value: 1 });

        const init = fetchMock.mock.calls[0][1] as RequestInit;
        expect(init.method).toBe('POST');
        expect(init.body).toBe('{"value":1}');
        expect((init.headers as Record<string, string>)['Content-Type']).toBe('application/json');
    });
});
