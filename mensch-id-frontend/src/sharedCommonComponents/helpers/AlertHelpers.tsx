import { confirmAlert } from "react-confirm-alert";
import { toast } from "react-toastify";
import { resolveText } from "./Globalizer";
import { ToastContent } from "../components/ToastContent";
import { ConfirmAlertOptions } from "../types/frontendTypes";

export const openConfirmAlert = (options: ConfirmAlertOptions) => {
    confirmAlert(options);
}
export const openConfirmDeleteAlert = (
    nameOfObjectToBeDeleted: string, 
    title: string, 
    message: string, 
    yesCallback: () => void, 
    noCallback: () => void = () => {}) => {        
    openConfirmAlert({
        title: title,
        message: message.replace('{0}', nameOfObjectToBeDeleted),
        closeOnClickOutside: true,
        buttons: [
            {
                label: resolveText('Delete_No'),
                onClick: noCallback
            }, 
            {
                label: resolveText('Delete_Yes'),
                onClick: yesCallback
            }
        ]
    });
}
export const showSuccessAlert = (title: string, message?: string, durationInMs?: number) => {
    toast.success(<ToastContent title={title} message={message} />, {
        autoClose: durationInMs
    });
}
export const showWarningAlert = (title: string, message?: string, durationInMs?: number) => {
    toast.warning(<ToastContent title={title} message={message} />, {
        autoClose: durationInMs
    });
}
export const showErrorAlert = (title: string, message?: string, durationInMs?: number) => {
    toast.error(<ToastContent title={title} message={message} />, {
        autoClose: durationInMs
    });
}
export const showInfoAlert = (title: string, message?: string, durationInMs?: number) => {
    toast.info(<ToastContent title={title} message={message} />, {
        autoClose: durationInMs
    });
}