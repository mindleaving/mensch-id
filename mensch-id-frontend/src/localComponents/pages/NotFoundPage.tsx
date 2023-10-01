import React from 'react';
import { Button } from 'react-bootstrap';
import { useNavigate } from 'react-router';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface NotFoundPageProps {}

export const NotFoundPage = (props: NotFoundPageProps) => {

    const navigate = useNavigate();
    return (
        <>
            <h1>{resolveText("PageNotFound")}</h1>
            <Button variant="link" onClick={() => navigate("/")}>
                &lt;&lt; Home
            </Button>
        </>
    );

}
export default NotFoundPage;