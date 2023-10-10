import React, { useState } from 'react';
import { Button } from 'react-bootstrap';
import { resolveText } from '../helpers/Globalizer';

interface CopyButtonProps {
    value: string;
    size?: "xs" | "sm" | "lg";
    className?: string;
}

export const CopyButton = (props: CopyButtonProps) => {

    const [ isCopied, setIsCopied ] = useState<boolean>(false);

    const copyToClipboard = () => {
        navigator.clipboard.writeText(props.value);
        setIsCopied(true);
        setTimeout(() => setIsCopied(false), 5000);
    }

    if(props.size === 'xs') {
        return (
            <span
                className={`${props.className ?? ''} ${isCopied ? 'text-success' : ''}`}
            >
                <i
                    className={`fa fa-files-o clickable`}
                    onClick={copyToClipboard}
                />
                {isCopied 
                ? <span className='ms-2'>
                    {resolveText("Copied!")}
                </span>
                : null}
            </span>
        );
    }

    return (
        <Button
            size={props.size}
            className={props.className}
            variant={isCopied ? 'success' : 'primary'}
            onClick={copyToClipboard}
        >
            {isCopied ? resolveText("Copied!") : resolveText("Copy")}
        </Button>
    );

}