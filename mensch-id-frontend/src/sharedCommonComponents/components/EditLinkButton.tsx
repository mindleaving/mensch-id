import React from 'react';
import { Button } from 'react-bootstrap';
import { resolveText } from '../helpers/Globalizer';

interface EditLinkButtonProps {
    onClick: () => void;
    className?: string;
}

export const EditLinkButton = (props: EditLinkButtonProps) => {

    return (
        <Button 
            variant="link" 
            onClick={props.onClick}
            className={props.className}
        >
            {resolveText('Edit...')}
        </Button>
    );

}