import { confirmAlert } from "react-confirm-alert";
import { resolveText } from "./Globalizer";

export const openConfirmDeleteAlert = (id: string, name: string, title: string, message: string, yesCallback: () => void, noCallback: () => void = () => {}) => {
    confirmAlert({
        title: title,
        message: message.replace('{0}', name),
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