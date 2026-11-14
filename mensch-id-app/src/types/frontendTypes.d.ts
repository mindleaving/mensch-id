export type AuthStatus = 'loading' | 'unavailable' | 'loggedOut' | 'loggedIn';

export interface TokenSet {
    accessToken: string;
    refreshToken?: string;
    idToken?: string;
}

export interface RefreshTokenStorage {
    load: () => Promise<string | null>;
    save: (refreshToken: string) => Promise<void>;
    loadIdToken: () => Promise<string | null>;
    saveIdToken: (idToken: string) => Promise<void>;
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

export interface AppLockSettings {
    pinHash?: string;
    pinSalt?: string;
    isBiometricsEnabled: boolean;
    failedPinAttempts: number;
    biometricUseCount: number;
    lastPinEntryTimestamp?: number;
}

export interface AppLockStorage {
    load: () => Promise<AppLockSettings>;
    save: (settings: AppLockSettings) => Promise<void>;
}

export interface AppLockDependencies {
    storage: AppLockStorage;
    hashPin: (pin: string, salt: string) => Promise<string>;
    createSalt: () => Promise<string>;
    now: () => number;
}

export type PinVerificationResult = 'valid' | 'invalid' | 'lockedOut';

export type LockTrigger = 'coldStart' | 'freshLogin';

export type AppLockStatus = 'loading' | 'setupRequired' | 'locked' | 'unlocked';

export interface AppLockContextValue {
    status: AppLockStatus;
    isBiometricsAvailable: boolean;
    isBiometricsEnabled: boolean;
    remainingPinAttempts: number;
    setInitialPin: (pin: string) => Promise<void>;
    unlockWithPin: (pin: string) => Promise<PinVerificationResult>;
    unlockWithBiometrics: () => Promise<'unlocked' | 'pinRequired' | 'failed'>;
    changePin: (currentPin: string, newPin: string) => Promise<PinVerificationResult>;
    setBiometricsEnabled: (isEnabled: boolean) => Promise<boolean>;
    setPinResetRequested: (isRequested: boolean) => void;
    runWithoutLock: <T>(action: () => Promise<T>) => Promise<T>;
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
