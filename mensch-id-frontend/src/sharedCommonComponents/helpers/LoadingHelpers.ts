import { apiClient } from "../communication/ApiClient";
import { QueryParameters } from "../types/frontendTypes";
import { showErrorAlert } from "./AlertHelpers";
import { handleResponse } from "./ApiResponseHandler";

export const buildLoadObjectFunc = <T>(
    apiPath: string,
    params: QueryParameters = {},
    errorText: string,
    onItemLoaded: (item: T) => void,
    onFailure?: (response: Response | undefined) => Promise<void>,
    onFinally?: () => void) => {
    return async () => await loadObject(apiPath, params, errorText, onItemLoaded, onFailure, onFinally);
}
export const loadObject = async <T>(
    apiPath: string,
    params: QueryParameters = {},
    errorText: string,
    onItemLoaded: (item: T) => void,
    onFailure?: (response: Response | undefined) => Promise<void>,
    onFinally?: () => void
) => {
    const onSuccess = async (response: Response) => {
        const item = await response.json() as T;
        onItemLoaded(item);
    }
    try {
        const response = await apiClient.instance!.get(apiPath, params, { handleError: false });
        await handleResponse(response, errorText, onSuccess, onFailure);
    } catch (error: any) {
        if(onFailure) {
            onFailure(undefined);
        } else {
            showErrorAlert(errorText, error.message);
        }
    } finally {
        if(onFinally) {
            onFinally();
        }
    }
}