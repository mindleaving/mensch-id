import { Alert } from 'react-bootstrap';
import { resolveText } from '../helpers/Globalizer';

interface NoEntriesAlertProps {}

export const NoEntriesAlert = (props: NoEntriesAlertProps) => {

    return (
        <Alert 
            variant="info" 
            className='text-center'
        >
            {resolveText("NoEntries")}
        </Alert>
    );

}