import { Button } from 'react-bootstrap';
import { resolveText } from '../helpers/Globalizer';

interface EditLinkButtonProps {
    onClick: () => void;
    className?: string;
    size?: 'xs' | 'sm' | 'lg';
}

export const EditLinkButton = (props: EditLinkButtonProps) => {

    const { onClick, className, size } = props;

    if(size === 'xs') {
        const cssClasses = [ 'fa', 'fa-edit', 'clickable' ];
        if(className) {
            cssClasses.push(className);
        }
        return (<i
            className={cssClasses.join(' ')}
            onClick={onClick}
        />);
    }

    return (
        <Button 
            variant="link" 
            size={size}
            onClick={onClick}
            className={className}
        >
            {resolveText('Edit...')}
        </Button>
    );

}