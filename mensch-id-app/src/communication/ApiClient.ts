import { AuthService } from '../auth/AuthService';
import { ApiRequestOptions, FetchFunction } from '../types/frontendTypes';

export class ApiError extends Error {
    constructor(public readonly statusCode: number, message: string) {
        super(message);
        this.name = 'ApiError';
    }
}

export class ApiClient {
    constructor(
        private readonly baseUrl: string,
        private readonly authService: AuthService,
        private readonly fetchFunction: FetchFunction = fetch
    ) {}

    get = async <T>(path: string): Promise<T> => {
        return await this.requestJson<T>(path, { method: 'GET' });
    };

    post = async <T>(path: string, body?: unknown): Promise<T> => {
        return await this.requestJson<T>(path, { method: 'POST', body: body });
    };

    request = async (path: string, options: ApiRequestOptions = {}): Promise<Response> => {
        if(!this.authService.hasAccessToken()) {
            await this.authService.refreshAccessToken();
        }
        const response = await this.send(path, options);
        if(response.status !== 401) {
            return response;
        }
        await this.authService.refreshAccessToken();
        return await this.send(path, options);
    };

    private requestJson = async <T>(path: string, options: ApiRequestOptions): Promise<T> => {
        const response = await this.request(path, options);
        if(!response.ok) {
            const errorText = await response.text();
            throw new ApiError(response.status, errorText || response.statusText);
        }
        return await response.json() as T;
    };

    private send = async (path: string, options: ApiRequestOptions): Promise<Response> => {
        const headers: Record<string, string> = {
            Accept: 'application/json'
        };
        const accessToken = this.authService.getAccessToken();
        if(accessToken) {
            headers.Authorization = `Bearer ${accessToken}`;
        }
        let body: string | undefined;
        if(options.body !== undefined) {
            headers['Content-Type'] = 'application/json';
            body = JSON.stringify(options.body);
        }
        const url = `${this.baseUrl}/${path.replace(/^\/+/, '')}`;
        return await this.fetchFunction(url, {
            method: options.method ?? 'GET',
            headers: headers,
            body: body
        });
    };
}
