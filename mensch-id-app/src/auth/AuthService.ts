import { RefreshTokenStorage, TokenEndpoints, TokenSet } from '../types/frontendTypes';

export class SessionExpiredError extends Error {
    constructor() {
        super('The session has expired');
        this.name = 'SessionExpiredError';
    }
}

export class InvalidRefreshTokenError extends Error {
    constructor() {
        super('The refresh token was rejected');
        this.name = 'InvalidRefreshTokenError';
    }
}

export class AuthService {
    private accessToken: string | undefined;
    private refreshPromise: Promise<string> | undefined;
    private readonly sessionEndedListeners = new Set<() => void>();
    private tokenEndpoints: TokenEndpoints | undefined;

    constructor(private readonly storage: RefreshTokenStorage) {}

    configure = (tokenEndpoints: TokenEndpoints): void => {
        this.tokenEndpoints = tokenEndpoints;
    };

    getAccessToken = (): string | undefined => {
        return this.accessToken;
    };

    hasAccessToken = (): boolean => {
        return this.accessToken !== undefined;
    };

    onSessionEnded = (listener: () => void): (() => void) => {
        this.sessionEndedListeners.add(listener);
        return () => {
            this.sessionEndedListeners.delete(listener);
        };
    };

    startSession = async (tokens: TokenSet): Promise<void> => {
        this.accessToken = tokens.accessToken;
        await this.storeTokens(tokens);
    };

    restoreSession = async (): Promise<boolean> => {
        const refreshToken = await this.storage.load();
        if(!refreshToken) {
            return false;
        }
        try {
            await this.refreshAccessToken();
            return true;
        } catch (error) {
            if(error instanceof SessionExpiredError) {
                return false;
            }
            throw error;
        }
    };

    refreshAccessToken = (): Promise<string> => {
        if(!this.refreshPromise) {
            this.refreshPromise = this.executeRefresh().finally(() => {
                this.refreshPromise = undefined;
            });
        }
        return this.refreshPromise;
    };

    endSession = async (): Promise<string | undefined> => {
        const refreshToken = await this.storage.load();
        const idToken = await this.storage.loadIdToken();
        this.accessToken = undefined;
        await this.storage.clear();
        if(refreshToken) {
            await this.tokenEndpoints?.revokeRefreshToken(refreshToken).catch(() => undefined);
        }
        this.notifySessionEnded();
        return idToken ?? undefined;
    };

    private storeTokens = async (tokens: TokenSet): Promise<void> => {
        if(tokens.refreshToken) {
            await this.storage.save(tokens.refreshToken);
        }
        if(tokens.idToken) {
            await this.storage.saveIdToken(tokens.idToken);
        }
    };

    private executeRefresh = async (): Promise<string> => {
        const refreshToken = await this.storage.load();
        if(!refreshToken) {
            await this.expireSession();
            throw new SessionExpiredError();
        }
        let tokens: TokenSet;
        try {
            tokens = await this.getTokenEndpoints().refreshTokens(refreshToken);
        } catch (error) {
            if(!(error instanceof InvalidRefreshTokenError)) {
                throw error;
            }
            await this.expireSession();
            throw new SessionExpiredError();
        }
        this.accessToken = tokens.accessToken;
        await this.storeTokens(tokens);
        return tokens.accessToken;
    };

    private getTokenEndpoints = (): TokenEndpoints => {
        if(!this.tokenEndpoints) {
            throw new Error('Token endpoints have not been configured');
        }
        return this.tokenEndpoints;
    };

    private expireSession = async (): Promise<void> => {
        this.accessToken = undefined;
        await this.storage.clear();
        this.notifySessionEnded();
    };

    private notifySessionEnded = (): void => {
        this.sessionEndedListeners.forEach(listener => listener());
    };
}
