import { resolveText } from '../helpers/Globalizer';
import { AsyncButton } from './AsyncButton';

interface StoreButtonProps {
    type?: "button" | "submit" | "reset";
    form?: string;
    onClick?: () => void;
    isStoring: boolean;
    isStored?: boolean;
    disabled?: boolean;
    size?: 'xs' | 'sm' | 'lg';
    className?: string;
}

export const StoreButton = (props: StoreButtonProps) => {

    const {
        type,
        form,
        onClick,
        isStoring,
        isStored,
        disabled,
        size,
        className
    } = props;

    if(size === 'xs' && !form && (!type || type === 'button')) {
        const classNames: string[] = [ 'fa' ];
        if(!disabled) {
            classNames.push('clickable');
        }
        if(className && className?.length > 0) {
            classNames.push(className);
        }
        if(isStoring) {
            classNames.push('fa-spinner');
            classNames.push('text-secondary');
        } else if(isStored) {
            classNames.push('fa-check');
            classNames.push('green');
        } else {
            classNames.push('fa-floppy-o');
            if(disabled) {
                classNames.push('text-secondary');
            } else {
                classNames.push('text-primary');
            }
        }
        return (<i 
            className={classNames.join(' ')}
            onClick={onClick}
        />);
    }

    return (
        <AsyncButton
            type={type}
            form={form}
            onClick={onClick}
            className={className ?? 'm-2'}
            activeText={isStored ? resolveText('Stored') : resolveText('Store')}
            executingText={resolveText('Storing...')}
            isExecuting={isStoring}
            disabled={disabled || isStored}
            size={size === 'xs' ? 'sm' : size}
            variant={isStored ? 'success' : 'primary'}
        />
    );

}