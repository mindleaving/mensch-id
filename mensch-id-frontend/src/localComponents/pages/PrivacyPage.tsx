import React from 'react';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface PrivacyPageProps {}

export const PrivacyPage = (props: PrivacyPageProps) => {

    return (
        <>
            <h1>{resolveText("Privacy")}</h1>
        </>
    );

}