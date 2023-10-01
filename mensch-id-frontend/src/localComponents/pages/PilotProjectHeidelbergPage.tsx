import React from 'react';
import { Image } from 'react-bootstrap';
import { Center } from '../../sharedCommonComponents/components/Center';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface PilotProjectHeidelbergPageProps {}

export const PilotProjectHeidelbergPage = (props: PilotProjectHeidelbergPageProps) => {

    return (
        <>
            <h1>{resolveText("PilotProject_Heidelberg")}</h1>
            <h5 className='mt-4'>English:</h5>
            <div className='lead'>
                mensch.ID is starting a pilot project in Heidelberg, Germany to test the system and work out issues, before scaling up. 
                All medical institutions and their partners, e.g. hospitals, labs, general practitioners, nursing homes, are invited to participate. 
                The text below describes the project, what it takes to participate and what it tries to achieve (German only).
            </div>
            <h5 className='mt-4'>German:</h5>
            <div className='lead'>
                mensch.ID startet ein Pilotprojekt in Heidelberg um das System zu testen und Kinderkrankheiten zu beheben 
                bevor es auf höherer Ebene zum Einsatz kommt.
                Alle medizinischen Einrichtungen und deren Partner, z.B. Krankenhäuser, Labore, Pflegeheime, Arztpraxen, sind eingeladen teilzunehmen.
                Die folgenden Abschnitte beschreiben das Projekt, wie man mitmachen kann und was es versucht zu erreichen.
            </div>
            <Center className='my-3'>
                <Image
                    src='/img/blood-sample-tube-mensch-id.jpg'
                    width={400}
                    alt="Blood sample tube with 웃ID on it"
                />
            </Center>
            <Center className='mt-5'>
                <ProjektFlyerLink className="btn btn-success btn-lg" />
            </Center>
            <h3>Das Projekt</h3>
            <p>
                Das Pilotprojekt in Heidelberg soll die Praxistauglichkeit des mensch.ID-Systems testen und dabei Probleme beheben und 
                die breitere Anwendung in Deutschland und Europa vorbereiten.
            </p>
            <p>
                Weitere Informationen sind im <ProjektFlyerLink /> zusammengefasst.
            </p>
            <p>
                Parallel zum Pilotprojekt wird mensch.ID nationalen und internationalen Projekten, Behörden und Politikern vorgestellt.
            </p>
            
            <h3>Teilnahme</h3>
            <p>
                Um mehr über die Teilnahme zu erfahren wird am <strong>29. September um 11:30-12:10 eine Online-Konferenz</strong> veranstaltet 
                an der alle Interessenten teilnehmen können um Fragen zu stellen und das Projekt zu diskutieren. 
                Der Link und andere Kontaktdaten sind im <ProjektFlyerLink /> zu finden
            </p>
            <p>
                Teilnehmende Einrichtungen die direkten Kontakt mit Patienten haben werden mit Flyern und Zugang zum Vergabeportal versorgt, 
                damit sie 웃ID vergeben können oder den Personen mit dem Flyer genug Informationen an die Hand geben um selber eine 웃ID zu erstellen.
            </p>
            <p>
                Vor Beginn der aktiven Phase (wenn 웃ID vergeben werden), sollten die teilnehmenden Eintrichtungen mit ihren Partnern absprechen 
                wann und wie die 웃ID sinnvoll eingesetzt werden kann.
            </p>
            <p>
                Nach der Online-Konferenz im September 2022 werden auf dieser Seite weitere Informationen zur Teilnahme auftauchen. 
                Ganz unabhängig davon bin ich immer per Mail erreichbar (siehe <ProjektFlyerLink />).
            </p>

            <h3>Ziele</h3>
            <p>
                Das Pilotprojekt soll
            </p>
            <ul>
                <li>die Teilnehmer und Umwelt für den Nutzen und die Einschränkungen der 웃ID sensibilisieren</li>
                <li>Probleme in der technischen Umsetzung aufdecken und beheben</li>
                <li>dokumentieren was für eine erfolgreiche Implementation der 웃ID notwendig ist</li>
                <li>rechtliche Fragen klären</li>
                <li>Meinungen und Reaktionen sammeln</li>
            </ul>
        </>
    );

}
const ProjektFlyerLink = (props: { className?: string; }) => (<a href="/docs/mensch-id_pilotprojekt-flyer_v1.pdf" className={props.className}>Projekt-Flyer</a>);

export default PilotProjectHeidelbergPage;