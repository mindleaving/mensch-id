import * as AuthSession from 'expo-auth-session';
import * as WebBrowser from 'expo-web-browser';
import {
    OidcClientId,
    OidcPostLogoutRedirectUri,
    OidcRedirectUri,
    OidcScopes
} from '../helpers/ConfigurationHelpers';
import { TokenSet } from '../types/frontendTypes';
import { InvalidRefreshTokenError } from './AuthService';

export class LoginCancelledError extends Error {
    constructor() {
        super('Login was cancelled');
        this.name = 'LoginCancelledError';
    }
}

const toTokenSet = (tokenResponse: AuthSession.TokenResponse): TokenSet => ({
    accessToken: tokenResponse.accessToken,
    refreshToken: tokenResponse.refreshToken
});

export const loginWithBrowser = async (
    discovery: AuthSession.DiscoveryDocument,
    loginProviderId?: string
): Promise<TokenSet> => {
    const extraParams: Record<string, string> = loginProviderId ? { provider: loginProviderId } : {};
    const request = new AuthSession.AuthRequest({
        clientId: OidcClientId,
        redirectUri: OidcRedirectUri,
        responseType: AuthSession.ResponseType.Code,
        scopes: OidcScopes,
        usePKCE: true,
        extraParams: extraParams
    });
    const result = await request.promptAsync(discovery);
    if(result.type !== 'success') {
        throw new LoginCancelledError();
    }
    const tokenResponse = await AuthSession.exchangeCodeAsync(
        {
            clientId: OidcClientId,
            code: result.params.code,
            redirectUri: OidcRedirectUri,
            extraParams: { code_verifier: request.codeVerifier ?? '' }
        },
        discovery
    );
    return toTokenSet(tokenResponse);
};

export const refreshTokens = async (
    discovery: AuthSession.DiscoveryDocument,
    refreshToken: string
): Promise<TokenSet> => {
    try {
        const tokenResponse = await AuthSession.refreshAsync(
            {
                clientId: OidcClientId,
                refreshToken: refreshToken
            },
            discovery
        );
        return toTokenSet(tokenResponse);
    } catch (error) {
        if(error instanceof AuthSession.TokenError && error.code === 'invalid_grant') {
            throw new InvalidRefreshTokenError();
        }
        throw error;
    }
};

export const revokeRefreshToken = async (
    discovery: AuthSession.DiscoveryDocument,
    refreshToken: string
): Promise<void> => {
    await AuthSession.revokeAsync(
        {
            clientId: OidcClientId,
            token: refreshToken,
            tokenTypeHint: AuthSession.TokenTypeHint.RefreshToken
        },
        discovery
    );
};

export const endBrowserSession = async (discovery: AuthSession.DiscoveryDocument): Promise<void> => {
    if(!discovery.endSessionEndpoint) {
        return;
    }
    const endSessionUrl = `${discovery.endSessionEndpoint}?client_id=${encodeURIComponent(OidcClientId)}`
        + `&post_logout_redirect_uri=${encodeURIComponent(OidcPostLogoutRedirectUri)}`;
    await WebBrowser.openAuthSessionAsync(endSessionUrl, OidcPostLogoutRedirectUri);
};
