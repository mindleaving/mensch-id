import { PropsWithChildren, useEffect, useState } from 'react';
import * as AuthSession from 'expo-auth-session';
import { AuthContext } from './AuthContext';
import { AuthService } from './AuthService';
import { secureRefreshTokenStorage } from './SecureRefreshTokenStorage';
import { endBrowserSession, loginWithBrowser, refreshTokens, revokeRefreshToken } from './OidcClient';
import { ApiClient } from '../communication/ApiClient';
import { ApiBaseUrl } from '../helpers/ConfigurationHelpers';
import { AuthContextValue, AuthStatus } from '../types/frontendTypes';

interface AuthProviderProps {}

export const AuthProvider = (props: PropsWithChildren<AuthProviderProps>) => {

    const [ authService ] = useState(() => new AuthService(secureRefreshTokenStorage));
    const [ apiClient ] = useState(() => new ApiClient(ApiBaseUrl, authService));
    const [ discovery, setDiscovery ] = useState<AuthSession.DiscoveryDocument | null>(null);
    const [ connectionAttempt, setConnectionAttempt ] = useState<number>(0);
    const [ status, setStatus ] = useState<AuthStatus>('loading');

    useEffect(() => {
        return authService.onSessionEnded(() => setStatus('loggedOut'));
    }, [ authService ]);

    useEffect(() => {
        let isCancelled = false;
        const connect = async () => {
            try {
                const loadedDiscovery = await AuthSession.fetchDiscoveryAsync(ApiBaseUrl + '/api/oidc');
                if(isCancelled) {
                    return;
                }
                authService.configure({
                    refreshTokens: refreshToken => refreshTokens(loadedDiscovery, refreshToken),
                    revokeRefreshToken: refreshToken => revokeRefreshToken(loadedDiscovery, refreshToken)
                });
                setDiscovery(loadedDiscovery);
                const isRestored = await authService.restoreSession();
                if(!isCancelled) {
                    setStatus(isRestored ? 'loggedIn' : 'loggedOut');
                }
            } catch {
                if(!isCancelled) {
                    setStatus('unavailable');
                }
            }
        };
        connect();
        return () => {
            isCancelled = true;
        };
    }, [ authService, connectionAttempt ]);

    const retryConnection = () => {
        setStatus('loading');
        setConnectionAttempt(attempt => attempt + 1);
    };

    const login = async (loginProviderId?: string) => {
        if(!discovery) {
            throw new Error('OpenID Connect discovery document not loaded');
        }
        const tokens = await loginWithBrowser(discovery, loginProviderId);
        await authService.startSession(tokens);
        setStatus('loggedIn');
    };

    const logout = async () => {
        await authService.endSession();
        if(discovery) {
            await endBrowserSession(discovery).catch(() => undefined);
        }
        setStatus('loggedOut');
    };

    const value: AuthContextValue = {
        status: status,
        isLoginAvailable: discovery !== null,
        apiClient: apiClient,
        login: login,
        logout: logout,
        retryConnection: retryConnection
    };

    return (
        <AuthContext.Provider value={value}>
            {props.children}
        </AuthContext.Provider>
    );
};
