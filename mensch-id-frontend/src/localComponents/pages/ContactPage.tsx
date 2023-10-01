import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

export const ContactPage = () => {

    return (
        <>
            <h1>{resolveText("Impressum")}</h1>

            <h2>{resolveText("Impressum_AccordingToLaw")}</h2>
            <p>
                Jan Scholtyssek (웃ID: 19891117-XMWT3)<br />
                Poststr. 28<br />
                69115 Heidelberg<br />
                Germany
            </p>

            <h2>{resolveText("Contact")}</h2>
            <p>
                {resolveText("Telephone")}: <a href='tel:+49 174 6322405'>+49 174 6322405</a><br />
                {resolveText("Email")}: <a href='mailto:janscholtyssek.dk'>jan@janscholtyssek.dk</a>
            </p>

            <p>
                <small>{resolveText("Source")}: <a href="https://www.e-recht24.de/impressum-generator.html">https://www.e-recht24.de/impressum-generator.html</a></small>
            </p>
        </>
    );

}
export default ContactPage;