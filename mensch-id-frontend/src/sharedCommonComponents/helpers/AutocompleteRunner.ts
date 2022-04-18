import { apiClient } from "../communication/ApiClient";

export class AutocompleteRunner<T> {
    url: string;
    searchParameter: string;
    maxSuggestions?: number;

    constructor(url: string, searchParameter: string, maxSuggestions?: number) {
        this.url = url;
        this.searchParameter = searchParameter;
        this.maxSuggestions = maxSuggestions;
    }

    search = async (searchText: string): Promise<T[]> => {
        const params = { 
            [this.searchParameter]: searchText, 
        };
        if(this.maxSuggestions) {
            params['count'] = this.maxSuggestions + '';
        }
        const response = await apiClient.instance!.get(this.url, params);
        const items = await response.json() as T[];
        return items;
    }
}