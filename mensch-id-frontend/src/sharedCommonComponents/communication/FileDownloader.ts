import { resolveText } from "../helpers/Globalizer";
import { apiClient } from "./ApiClient";
import { removeSurroundingQuotes } from "../helpers/StringExtensions";
import { showErrorAlert } from "../helpers/AlertHelpers";

export const downloadFile = async (url: string) => {
    try {
        const response = await apiClient.instance!.get(url);
        const result = await response.blob();
        const contentDispositionHeader = response.headers.get("content-disposition");
        const filenameFromHeader = removeSurroundingQuotes(contentDispositionHeader
            ?.split(';')
            .map(x => x.trim())
            .find(x => x.toLowerCase().startsWith("filename="))
            ?.split('=')[1]
        );
        const filename = filenameFromHeader ?? 'document.bin';
        downloadLocalData(result, filename);
    } catch(error: any) {
        showErrorAlert(resolveText("Download_CouldNotDownload"), error.message);
    }
}
export const downloadLocalData = (data: Blob | MediaSource, filename: string) => {
    const anchor = document.createElement("a");
    document.body.appendChild(anchor);
    try {
        const objectUrl = window.URL.createObjectURL(data);
        anchor.href = objectUrl;
        anchor.download = filename;
        anchor.click();
        window.URL.revokeObjectURL(objectUrl);
    } finally {
        document.body.removeChild(anchor);
    }
}