import { resolveText } from "../helpers/Globalizer";
import { NotificationManager } from 'react-notifications';
import { apiClient } from "./ApiClient";

export const downloadFile = async (url: string) => {
    const anchor = document.createElement("a");
    document.body.appendChild(anchor);
    try {
        const response = await apiClient.instance!.get(url, {});
        const result = await response.blob();
        response.headers.forEach(console.log);
        const contentDispositionHeader = response.headers.get("content-disposition");
        console.log(contentDispositionHeader);
        const filename = contentDispositionHeader?.split(';').map(x => x.trim())
            .find(x => x.toLowerCase().startsWith("filename="))?.split('=')[1]
            ?? 'document.bin';
        const objectUrl = window.URL.createObjectURL(result);
        anchor.href = objectUrl;
        anchor.download = filename;
        anchor.click();
        window.URL.revokeObjectURL(objectUrl);
    } catch(error: any) {
        NotificationManager.error(error.message, resolveText("Download_CouldNotDownload"));
    } finally {
        document.body.removeChild(anchor);
    }
}