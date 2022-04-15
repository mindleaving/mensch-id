import React from 'react';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface NotFoundPageProps {}

export const NotFoundPage = (props: NotFoundPageProps) => {

    return (
        <>
            <h1>{resolveText("PageNotFound")}</h1>
        </>
    );

}