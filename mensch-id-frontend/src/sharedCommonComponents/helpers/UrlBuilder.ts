export function buildUrl(endpoint: string, path: string, params: { [key: string]: string | undefined }) {
    let url = endpoint.endsWith('/') 
        ? endpoint.substr(0, endpoint.length-1) 
        : endpoint;
    if(!path.startsWith('/')) {
        url += '/' + path;
    } else {
        url += path;
    }
    let firstQueryParameter = true;
    for(const name in params) {
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
    return url;
}