import React from 'react';
import { Accordion } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { AccordionCard } from '../../../sharedCommonComponents/components/AccordionCard';

export const FAQ_DE = () => {

    return (
        <Accordion>
            <AccordionCard
                title="Welche Informationen werden gesammelt?"
                eventKey='faq-q1'
            >
                <p>Wenn du ein anonymes Konto benutzt: Nur dein Geburtsdatum (das du dir mit 100-140 Tausend anderen Menschen teilst)</p>
                <p>Wen du dich mit einem externen Anbieter (Google, Facebook, Microsoft oder Twitter) anmeldest, wird deine öffentliche Konto-ID wie ein Passwort gespeichert. Dadurch kann die Seite dich wiedererkennen wenn du dich anmeldest, die Konto-ID aber nicht wiederhergestellt werden selbst wenn die Datenbank gestohlen wird.</p>
                <p>Wenn du ein nicht-anonymes Konto benutzt wird außerdem deine Email-Adresse gespeichert die für die Passwortwiederherstellung benutzt wird, wenn du dein Passwort vergessen hast</p>
                <p><small>Technische Details: Passwörter und externe Konto-IDs werden mit einem State-of-the-art Schlüsselerzeugungsalgorithmus (PBKDF2) gespeichert. Externe Konto-IDs werden zusätzlich mit einem Geheimwort versehen um Brute-force-Attacken (ausprobieren aller möglichen IDs) zu unterbinden. Du kannst den Code auf <a href="https://github.com/mindleaving/mensch-id" target="_blank" rel="noreferrer">Github</a> einsehen.</small></p>
            </AccordionCard>
            <AccordionCard
                title="Was ist eine Challenge?"
                eventKey='faq-q2'
            >
                <p>Eine Challenge ist eine relativ benutzerfreundliche und einigermaßen sichere Art zu verifizieren dass jemand eine bestimmte 웃ID besitzt.</p>
                <p>
                    Eine Challenge ist notwendig weil eine 웃ID an sich nicht geheim ist (meine ist auf der Impressumsseite angegeben) 
                    und leicht zu fälschen ist.
                </p>
                <p>Eine Challenge wird folgendermaßen ausgeführt:</p>
                <ol>
                    <li>Besuche die <Link to="/challenge">Challenge-Seite</Link></li>
                    <li>Gebe die zu verifizierende 웃ID ein</li>
                    <li>Gib der anderen Person die <strong>Challenge-ID</strong>. <span className='red'>Behalte das Geheimwort für dich!</span></li>
                    <li>Dein Gegenüber geht nun auf seine private <Link to="/challenges">Challenge-Seite</Link> und sucht dort nach der Challenge-ID</li>
                    <li>Dein Gegenüber liest dir das Geheimwort vor / schickt es dir zu</li>
                    <li>Wenn das Geheimwort stimmt, hat die Person bewiesen dass sie im Besitz dieser 웃ID ist</li>
                </ol>
            </AccordionCard>
            <AccordionCard
                title="Warum brauche ich/wir eine mensch.ID?"
                eventKey='faq-q3'
            >
                <p>
                    Eine Identifikationsnummer (ID) ist notwendig um eine Person (oder ein Objekt) in verschiedenen Bereichen eindeutig identifizieren zu können.
                    Die Verwendung von Name + Geburtsdatum + Ort ist nicht nur umständlich, sondern nicht garantiert eindeutig und bei Namensänderungen problematisch.
                </p>
                <p>
                    Ein Beispiel einer Situation wo eine eindeutige Identifikation wichtig ist, 
                    ist der Austausch deiner medizinischen Daten zwischen dem Krankenhaus und deinem Hausarzt.
                    Um sicherzustellen dass das Krankenhaus nur deine Daten und nicht die von jemand anderem mit gleichem oder ähnlichen Namen schickt, muss es dich eindeutig identifizieren.
                    Das wird in verschiedenen Ländern unterschiedlich gelöst. Mal durch eine Versichertennnummer, mal durch eine Kombination von Name, Geburtsdatum und -ort.
                </p>
                <p>
                    Aber heute reisen wir mehr und beim Austausch von Daten über Grenzen hinweg (und sei es nur die scheinbar unüberwindliche Grenze zwischen zwei Behörden) braucht es wieder eine ID.
                    Hier möchte ich mit 웃ID die Lücke schließen, um die Datensicherheit und die Beweglichkeit unserer Daten erhöhen. 
                    Ultimativ mit dem Ziel eines einfacheren und glücklicheren Lebens.
                </p>
            </AccordionCard>
            <AccordionCard
                title="Wie weit verbreitet ist 웃ID?"
                eventKey='faq-q4'
            >
                <p>웃ID wurde im September 2022 öffentlich zugänglich gemacht wird im Moment verschiedenen Projekten, Behörden und Entscheidungsträgern in Deutschland und der EU vorgestellt.</p>
                <p>Außerdem ist ein <Link to="/pilot-project-hd">Pilotprojekt in Heidelberg</Link> geplant, das Anfang 2023 beginnen soll 웃ID zu verwenden.</p>
                <p>
                    Obwohl 웃ID noch nicht breite Anwendung findet, kannst du das Projekt unterstützen indem du dir deine 웃ID erstellst 
                    und im Gesundheitswesen und bei Behörden angibst. Erkläre dort dass es eine eindeutige Identifikationsnummer ist und die gerne genutzt werden darf. 
                    Verweise auch gerne auf diese Internetseite.
                </p>
            </AccordionCard>
        </Accordion>
    );

}