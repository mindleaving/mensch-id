import React from 'react';
import { Alert } from 'react-bootstrap';
import { resolveText } from '../helpers/Globalizer';

interface CouldNotLoadAlertProps {
    errorText?: string;
}

export const CouldNotLoadAlert = (props: CouldNotLoadAlertProps) => {

    return (
        <Alert
            variant='danger'
        >
            {props.errorText ?? resolveText("CouldNotLoad")}
        </Alert>
    );

}