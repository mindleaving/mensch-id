export type AuthStatus = 'loading' | 'unavailable' | 'loggedOut' | 'loggedIn';

export interface TokenSet {
    accessToken: string;
    refreshToken?: string;
}

export interface RefreshTokenStorage {
    load: () => Promise<string | null>;
    save: (refreshToken: string) => Promise<void>;
    clear: () => Promise<void>;
}

export interface TokenEndpoints {
    refreshTokens: (refreshToken: string) => Promise<TokenSet>;
    revokeRefreshToken: (refreshToken: string) => Promise<void>;
}

export interface ApiRequestOptions {
    method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
    body?: unknown;
}

export type FetchFunction = (input: string, init?: RequestInit) => Promise<Response>;

export interface ChallengeSearchState {
    isSearching: boolean;
    hasError: boolean;
    challenges: import('./models').Models.MenschIdChallenge[];
}

export interface ChallengeSearchResult {
    searchText: string;
    hasError: boolean;
    challenges: import('./models').Models.MenschIdChallenge[];
}

export interface ProfileState {
    status: 'loading' | 'loaded' | 'noProfile' | 'error';
    menschId?: string;
}

export interface AuthContextValue {
    status: AuthStatus;
    isLoginAvailable: boolean;
    apiClient: import('../communication/ApiClient').ApiClient;
    login: (loginProviderId?: string) => Promise<void>;
    logout: () => Promise<void>;
    retryConnection: () => void;
}
