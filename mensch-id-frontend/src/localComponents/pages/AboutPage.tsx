import { VerbatimText } from '../../sharedCommonComponents/components/VerbatimText';
import { defaultGlobalizer, resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { FAQ_DE } from '../components/FAQ/FAQ_DE';
import { FAQ_EN } from '../components/FAQ/FAQ_EN';
import { Language } from '../types/enums.d';

export const AboutPage = () => {

    return (
        <>
            <h1>{resolveText("About")}</h1>
            <div className='lead'>
                <VerbatimText
                    text={resolveText("About_ElevatorPitch")}
                />
            </div>
            <hr />
            <h3>{resolveText("FAQ")}</h3>
            {defaultGlobalizer.instance!.preferedLanguage === Language.de ? <FAQ_DE />
            : <FAQ_EN />}
        </>
    );

}