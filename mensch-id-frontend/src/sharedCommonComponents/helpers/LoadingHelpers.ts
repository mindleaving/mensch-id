import { apiClient } from "../communication/ApiClient";
import { NotificationManager } from 'react-notifications';

export const buildLoadObjectFunc = <T extends unknown>(
    apiPath: string,
    params: { [key:string]: string } = {},
    errorText: string,
    onItemLoaded: (item: T) => void,
    onFailure?: () => void,
    onFinally?: () => void) => {
    return async () => await loadObject(apiPath, params, errorText, onItemLoaded, onFailure, onFinally);
}
export const loadObject = async <T extends unknown>(
    apiPath: string,
    params: { [key:string]: string } = {},
    errorText: string,
    onItemLoaded: (item: T) => void,
    onFailure?: () => void,
    onFinally?: () => void
) => {
    try {
        const response = await apiClient.instance!.get(apiPath, params);
        const item = await response.json() as T;
        onItemLoaded(item);
    } catch (error: any) {
        if(onFailure) {
            onFailure();
        }
        NotificationManager.error(error.message, errorText);
    } finally {
        if(onFinally) {
            onFinally();
        }
    }
}