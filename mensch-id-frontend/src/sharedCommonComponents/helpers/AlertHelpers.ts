import { confirmAlert } from "react-confirm-alert";
import { resolveText } from "./Globalizer";
import { NotificationManager } from 'react-notifications';

export const openConfirmDeleteAlert = (
    nameOfObjectToBeDeleted: string, 
    title: string, 
    message: string, 
    yesCallback: () => void, 
    noCallback: () => void = () => {}) => {        
    confirmAlert({
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
    if(title && message && durationInMs) {
        NotificationManager.success(message, title, durationInMs);
    }
    if(title && message) {
        NotificationManager.success(message, title);
    }
    NotificationManager.success(title);
}
export const showWarningAlert = (title: string, message?: string, durationInMs?: number) => {
    if(title && message && durationInMs) {
        NotificationManager.warning(message, title, durationInMs);
    }
    if(title && message) {
        NotificationManager.warning(message, title);
    }
    NotificationManager.warning(title);
}
export const showErrorAlert = (title: string, message?: string, durationInMs?: number) => {
    if(title && message && durationInMs) {
        NotificationManager.error(message, title, durationInMs);
    }
    if(title && message) {
        NotificationManager.error(message, title);
    }
    NotificationManager.error(title);
}
export const showInfoAlert = (title: string, message?: string, durationInMs?: number) => {
    if(title && message && durationInMs) {
        NotificationManager.info(message, title, durationInMs);
    }
    if(title && message) {
        NotificationManager.info(message, title);
    }
    NotificationManager.info(title);
}