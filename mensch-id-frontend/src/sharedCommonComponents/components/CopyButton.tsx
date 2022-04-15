import React, { useState } from 'react';
import { Button } from 'react-bootstrap';
import { resolveText } from '../helpers/Globalizer';

interface CopyButtonProps {
    value: string;
    size?: 'sm' | 'lg',
    className?: string;
}

export const CopyButton = (props: CopyButtonProps) => {

    const [ isCopied, setIsCopied ] = useState<boolean>(false);
    const copy = () => {
        navigator.clipboard.writeText(props.value);
        setIsCopied(true);
        setTimeout(() => setIsCopied(false), 5000);
    }
    return (
        <Button
            className={props.className}
            variant={isCopied ? 'success' : 'primary'}
            size={props.size ?? 'sm'}
            onClick={copy}
        >
            {isCopied ? resolveText("Copied!") : resolveText("Copy")}
        </Button>
    );

}