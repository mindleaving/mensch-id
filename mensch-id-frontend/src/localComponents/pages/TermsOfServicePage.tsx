import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface TermsOfServicePageProps {}

export const TermsOfServicePage = (props: TermsOfServicePageProps) => {

    return (
    <>
        <h1>{resolveText("TermsOfService")}</h1>
        <p>
            Don't be evil...
        </p>
    </>
    );

}