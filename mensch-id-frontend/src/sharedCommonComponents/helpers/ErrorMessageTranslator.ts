import { resolveText } from "./Globalizer";

export const translateErrorMessage = (errorText: string) => {
    return errorText.startsWith("resolveText:")
        ? resolveText(errorText.replace("resolveText:", ""))
        : errorText;
}