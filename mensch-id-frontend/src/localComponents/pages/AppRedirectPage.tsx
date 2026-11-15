import { useEffect } from 'react';
import { Alert } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { removeSensitiveQueryParameters } from '../helpers/AppRedirectHelpers';

interface AppRedirectPageProps {}

export const AppRedirectPage = (props: AppRedirectPageProps) => {

    useEffect(() => {
        removeSensitiveQueryParameters();
    }, []);

    return (
        <>
            <h1>{resolveText("AppRedirect_Title")}</h1>
            <Alert variant="info">{resolveText("AppRedirect_Text")}</Alert>
        </>
    );

};
export default AppRedirectPage;
