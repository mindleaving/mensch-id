import { apiClient } from "../communication/ApiClient";
import { QueryParameters } from "../types/frontendTypes";
import { showSuccessAlert, showErrorAlert } from "./AlertHelpers";
import { translateErrorMessage } from "./ErrorMessageTranslator";

export const buildAndStoreObject = async <T extends unknown>(
    apiPath: string,
    params: QueryParameters,
    successText: string,
    errorText: string,
    itemBuilder: () => T,
    onSuccess?: (item: T) => void,
    onFinally?: () => void
) => {
    try {
        const item = itemBuilder();
        await apiClient.instance!.put(apiPath, item, params);
        showSuccessAlert(successText);
        if(onSuccess) {
            onSuccess(item);
        }
    } catch(error: any) {
        showErrorAlert(errorText, error.message);
    } finally {
        if(onFinally) {
            onFinally();
        }
    }
}

export const sendPutRequest = async <T extends unknown>(
    apiPath: string,
    params: QueryParameters,
    errorText: string,
    item: T,
    handleResponse?: (response: Response) => void,
    onFailure?: () => void,
    onFinally?: () => void
) => {
    try {
        const response = await apiClient.instance!.put(apiPath, item, params);
        if(handleResponse) {
            handleResponse(response);
        }
    } catch(error: any) {
        if(onFailure) {
            onFailure();
        }
        showErrorAlert(errorText, error.message);
    } finally {
        if(onFinally) {
            onFinally();
        }
    }
}

export const sendPostRequest = async (
    apiPath: string,
    params: QueryParameters,
    errorText: string,
    body: any,
    onSuccess?: (response: Response) => void,
    onFailure?: (response: Response | undefined) => void,
    onFinally?: () => void
) => {
    try {
        const response = await apiClient.instance!.post(apiPath, body, params, { handleError: false });
        if(response.ok) {
            if(onSuccess) {
                onSuccess(response);
            }
        } else {
            if(onFailure) {
                onFailure(response);
            } else {
                try {
                    const errorDescription = await response.text();
                    const translatedErrorText = translateErrorMessage(errorDescription);
                    showErrorAlert(errorText, translatedErrorText);
                } catch {
                    showErrorAlert(errorText);
                    // Ignore
                }
            }
        }
    } catch(error: any) {
        if(onFailure) {
            onFailure(undefined);
        }
        showErrorAlert(errorText, error.message);
    } finally {
        if(onFinally) {
            onFinally();
        }
    }
}
export const uploadFile = (
    file: File,
    url: string,
    options?: {
        method?: "POST" | "PUT"
        contentType?: string,
        onProgressChanged?: (progress: number) => void
    }): Promise<number> => {

    const xhr = new XMLHttpRequest();
    xhr.withCredentials = true;
    return new Promise((resolve,reject) => {
        xhr.upload.addEventListener("progress", e => {
            if(e.lengthComputable && options?.onProgressChanged) {
                options.onProgressChanged(100 * e.loaded / e.total);
            }
        });
        xhr.addEventListener("loadend", () => {
            if(xhr.status === 200) {
                resolve(xhr.status);
            } else {
                reject();
            }
        })
        xhr.open(options?.method ?? "POST", url, true);
        xhr.setRequestHeader("Content-Type", options?.contentType ?? "application/octet-stream");
        xhr.send(file);
    });
}