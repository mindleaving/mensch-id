import { defaultGlobalizer } from '../../sharedCommonComponents/helpers/Globalizer';
import { TermsAndConditionsDE } from '../components/TermsAndConditions/TermsAndConditions_DE';
import { TermsAndConditionsEN } from '../components/TermsAndConditions/TermsAndConditions_EN';
import { Language } from '../types/enums';

interface TermsOfServicePageProps {}

export const TermsOfServicePage = (props: TermsOfServicePageProps) => {

    return (
    <>
        {defaultGlobalizer.instance!.preferedLanguage === Language.de ? <TermsAndConditionsDE />
        : <TermsAndConditionsEN />}
    </>
    );

}
export default TermsOfServicePage;