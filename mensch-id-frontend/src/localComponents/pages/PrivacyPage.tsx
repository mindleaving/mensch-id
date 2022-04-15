import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface PrivacyPageProps {}

export const PrivacyPage = (props: PrivacyPageProps) => {

    return (
        <>
            <h1>{resolveText("Privacy")}</h1>
            <h2>{resolveText("Privacy_WhatIsBeingStored_Title")}</h2>
            <p>
                {resolveText("Privacy_WhatIsBeingStored_Text")}
            </p>
            <h2>{resolveText("Privacy_HowIsDataUsed_Title")}</h2>
            <p>
                {resolveText("Privacy_HowIsDataUsed_Text")}
            </p>
        </>
    );

}