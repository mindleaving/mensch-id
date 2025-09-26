import { QueryParameters } from "../types/frontendTypes";

export const getQueryParameter = (params: QueryParameters, key: string) => {
    if(!params) {
        return undefined;
    }
    if(Array.isArray(params)) {
        return params.find(x => x.key === key)?.value;
    }
    return params[key];
}