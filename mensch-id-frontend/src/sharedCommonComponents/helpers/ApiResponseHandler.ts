import { showErrorAlert } from "./AlertHelpers";
import { translateErrorMessage } from "./ErrorMessageTranslator";

export const handleResponse = async (
    response: Response,
    errorText: string,
    onSuccess?: (response: Response) => Promise<void>,
    onFailure?: (response: Response | undefined) => Promise<void>) => {

    if(response.ok) {
        if(onSuccess) {
            await onSuccess(response);
        }
    } else {
        if(onFailure) {
            await onFailure(response);
        } else {
            try {
                const errorDescription = await response.text();
                const translatedErrorText = translateErrorMessage(errorDescription);
                showErrorAlert(errorText, translatedErrorText);
            } catch {
                showErrorAlert(errorText);
            }
        }
    }
}