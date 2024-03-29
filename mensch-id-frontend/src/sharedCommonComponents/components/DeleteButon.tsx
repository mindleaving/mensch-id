import { confirmAlert } from 'react-confirm-alert';
import { resolveText } from '../helpers/Globalizer';
import { AsyncButton } from './AsyncButton';

interface DeleteButtonProps {
    isDeleting: boolean;
    onClick: (isConfirmed?: boolean) => void;
    requireConfirm: boolean;
    confirmDialogTitle?: string;
    confirmDialogMessage?: string;
    type?: "button" | "submit" | "reset";
    size?: "sm" | "lg";
    className?: string;
}

export const DeleteButton = (props: DeleteButtonProps) => {

    if(props.requireConfirm && !(props.confirmDialogTitle && props.confirmDialogMessage)) {
        throw new Error("If delete confirmation is required, title and message for the confirmation dialog must be specified");
    }

    const confirmDelete = () => {
        confirmAlert({
            title: props.confirmDialogTitle,
            message: props.confirmDialogMessage,
            closeOnClickOutside: true,
            buttons: [
                {
                    label: resolveText('Delete_No'),
                    onClick: () => {}
                },
                {
                    label: resolveText('Delete_Yes'),
                    onClick: () => props.onClick(true)
                }
            ]
        });
    }

    return (
        <AsyncButton
            type={props.type}
            variant="danger"
            size={props.size}
            className={props.className ?? 'm-2'}
            activeText={resolveText('Delete')}
            executingText={resolveText('Deleting...')}
            isExecuting={props.isDeleting}
            onClick={props.requireConfirm ? confirmDelete : () => props.onClick(false)}
        />
    );

}