import React from 'react';
import { Alert } from 'react-bootstrap';
import { resolveText } from '../helpers/Globalizer';

interface NoMoreEntriesAlertProps {
    variant?: string;
}

export const NoMoreEntriesAlert = (props: NoMoreEntriesAlertProps) => {

    return (
        <Alert
            variant={props.variant ?? 'info'}
            className='text-center'
        >
            <strong>{resolveText("NoMoreEntries")}</strong>
        </Alert>
    );

}