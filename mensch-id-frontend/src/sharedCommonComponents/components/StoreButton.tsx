import { resolveText } from '../helpers/Globalizer';
import { AsyncButton } from './AsyncButton';

interface StoreButtonProps {
    type?: "button" | "submit" | "reset";
    form?: string;
    onClick?: () => void;
    isStoring: boolean;
    isStored?: boolean;
    disabled?: boolean;
    size?: 'sm' | 'lg';
    className?: string;
}

export const StoreButton = (props: StoreButtonProps) => {

    return (
        <AsyncButton
            type={props.type}
            form={props.form}
            onClick={props.onClick}
            className={props.className ?? 'm-2'}
            activeText={props.isStored ? resolveText('Stored') : resolveText('Store')}
            executingText={resolveText('Storing...')}
            isExecuting={props.isStoring}
            disabled={props.disabled || props.isStored}
            size={props.size}
            variant={props.isStored ? 'success' : 'primary'}
        />
    );

}