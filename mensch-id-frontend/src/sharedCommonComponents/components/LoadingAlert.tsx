import React from 'react';
import { Alert } from 'react-bootstrap';
import { resolveText } from '../helpers/Globalizer';

interface LoadingAlertProps {
    variant?: string;
}

export const LoadingAlert = (props: LoadingAlertProps) => {

    return (
        <Alert
            variant={props.variant ?? 'info'}
        >
            {resolveText("Loading...")}
        </Alert>
    );

}