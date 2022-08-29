import { resolveText } from '../helpers/Globalizer';
import { buildUrl } from '../helpers/UrlBuilder';
import { ApiError } from './ApiError';

export interface ApiClientOptions {
    handleError?: boolean;
    contentType?: string;
    includeCredentials?: boolean;
    stringifyBody?: boolean;
}
export class ApiClient {
    serverAddress: string;
    port: number;
    accessToken: string | undefined;
    defaultOptions: ApiClientOptions;

    constructor(serverAddress: string, port: number) {
        this.serverAddress = serverAddress;
        this.port = port;
        this.accessToken = undefined;
        this.defaultOptions = { 
            handleError: true, 
            contentType: 'application/json', 
            includeCredentials: false,
            stringifyBody: true
        };
    }

    isLoggedIn = async () => {
        const response = await this.get('api/logins/is-logged-in', {}, { handleError: false });
        if(response.ok) {
            return true;
        }
        if(response.status === 401) {
            return false;
        }
        this._handleError(response);
        return false;
    }

    get = async (path: string, params: { [key: string]: string }, options?: ApiClientOptions) => {
        return await this._sendRequest("GET", path, params, undefined, options);
    }

    put = async (path: string, params: { [key: string]: string }, body: any, options?: ApiClientOptions) => {
        return await this._sendRequest("PUT", path, params, body, options);
    }

    post = async (path: string, params: { [key: string]: string }, body: any, options?: ApiClientOptions) => {
        return await this._sendRequest("POST", path, params, body, options);
    }

    patch = async (path: string, params: { [key: string]: string }, body: any, options?: ApiClientOptions) => {
        return await this._sendRequest("PATCH", path, params, body, options);
    }

    delete = async (path: string, params: { [key: string]: string }, options?: ApiClientOptions) => {
        return await this._sendRequest("DELETE", path, params, undefined, options);
    }

    setAccessToken = (accessToken: string) => {
        this.accessToken = accessToken;
    }

    buildUrl = (path: string, params: {}) => {
        return buildUrl(`https://${this.serverAddress}:${this.port}`, path, params);
    }

    _sendRequest = async (
        method: "GET" | "POST" | "PUT" | "PATCH" | "DELETE",
        path: string, 
        params: { [key: string]: string },
        body?: any,
        options?: ApiClientOptions) => {

        const effectiveOptions = Object.assign({}, this.defaultOptions, options ?? {});
        const requestUrl = this.buildUrl(path, params);
        const jsonBody = body && effectiveOptions.stringifyBody ? this._convertToJson(body) : body;
        const headers: HeadersInit = {};
        if(effectiveOptions.contentType) {
            headers['Content-Type'] = effectiveOptions.contentType;
        }
        if(this.accessToken) {
            headers['Authorization'] = `Bearer ${this.accessToken}`;
        }
        const response = await fetch(requestUrl, {
            method: method,
            body: jsonBody,
            headers: headers,
            credentials: effectiveOptions.includeCredentials ? 'include' : 'omit'
        });
        if(effectiveOptions.handleError && !response.ok) {
            return await this._handleError(response);
        }
        return response;
    }

    _convertToJson = (body: any, isFile: boolean = false) => {
        if(typeof body === "string" || isFile) {
            return body;
        }
        if(typeof body === "object") {
            return JSON.stringify(body);
        }
        throw new Error(`Body to be sent to server must be either of type 'object' or a JSON-string, but was ${typeof body}`);
    }

    _handleError = async (response: Response) => {
        const errorText = await response.text();
        if(errorText.startsWith('{') && errorText.endsWith('}')) {
            const errorObject = JSON.parse(errorText);
            const errorsToken = errorObject['errors'];
            if(errorsToken) {
                const errors = Array.isArray(errorsToken)
                    ? (errorsToken as any[]).flatMap(x => x) as string[]
                    : [ errorText ];
                throw new ApiError(response.status, errors.join(', '));
            }
        }
        const translatedErrorText = errorText.startsWith("resolveText:")
            ? resolveText(errorText.replace("resolveText:", ""))
            : errorText;
        throw new ApiError(response.status, translatedErrorText);
    }
}

export const apiClient: { instance?: ApiClient } = {};