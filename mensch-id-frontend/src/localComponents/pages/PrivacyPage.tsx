import { defaultGlobalizer, resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { PrivacyPolicyDe } from '../components/PrivacyPolicy/PrivacyPolicy_DE';
import { PrivacyPolicyEn } from '../components/PrivacyPolicy/PrivacyPolicy_EN';
import { Language } from '../types/enums';

export const PrivacyPage = () => {

    return (
        <>
            <h1>{resolveText("Privacy")}</h1>
            {defaultGlobalizer.instance!.preferedLanguage === Language.de ? <PrivacyPolicyDe />
            : <PrivacyPolicyEn />}
        </>
    );

}
export default PrivacyPage;