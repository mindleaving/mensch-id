import { NotificationManager } from 'react-notifications';
import { apiClient } from '../communication/ApiClient';

export const deleteObject = async (
    apiPath: string,
    params: { [key: string]: string },
    successText: string,
    errorText: string,
    onSuccess?: () => void,
    onFinally?: () => void
) => {
    try {
        await apiClient.instance!.delete(apiPath, params);
        NotificationManager.success(successText);
        if(onSuccess) {
            onSuccess();
        }
    } catch(error: any) {
        NotificationManager.error(error.message, errorText);
    } finally {
        if(onFinally) {
            onFinally();
        }
    }
}