import { QueryParameters } from "../types/frontendTypes";

export function buildUrl(endpoint: string, path: string, params?: QueryParameters) {
    let url = endpoint.endsWith('/') 
        ? endpoint.substring(0, endpoint.length-1) 
        : endpoint;
    if(!path.startsWith('/')) {
        url += '/' + path;
    } else {
        url += path;
    }
    if(params) {
        let firstQueryParameter = true;
        if(Array.isArray(params)) {
            for (const parameter of params) {
                const value = parameter.value;
                if(value) {
                    if(firstQueryParameter) {
                        url += '?';
                        firstQueryParameter = false;
                    } else {
                        url += '&';
                    }
                    url += `${encodeURIComponent(parameter.key)}=${encodeURIComponent(value)}`;
                }
            }
        } else {
            for (const name in params) {
                const value = params[name];
                if(value) {
                    if(firstQueryParameter) {
                        url += '?';
                        firstQueryParameter = false;
                    } else {
                        url += '&';
                    }
                    url += `${encodeURIComponent(name)}=${encodeURIComponent(value)}`;
                }
            }
        }
    }
    return url;
}