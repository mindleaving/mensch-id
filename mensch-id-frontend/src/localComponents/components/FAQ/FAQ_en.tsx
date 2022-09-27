import React from 'react';
import { Accordion } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { AccordionCard } from '../../../sharedCommonComponents/components/AccordionCard';

export const FAQ_EN = () => {

    return (
        <Accordion>
            <AccordionCard
                title="What data is being collected?"
                eventKey='faq-q1'
            >
                <p>If you choose to use an anonymous account: Only your birthdate (which you share with 100-150k people).</p>
                <p>If using an external login provider, like Google, Facebook, Microsoft or Twitter, your public account ID is stored but in a way similar to a password, enabling the site to recognize you when you log in, but the external account ID is not recoverable even if the database should get stolen.</p>
                <p>If you choose a non-anonymous local account your email is stored to enable password resets.</p>
                <p><small>Technical details: Passwords and external account IDs are stored using a state-of-the-art key derivation (PBKDF2) using a salt. External account IDs are also prefixed with a secret (not stored in database) to mitigate brute-forcing. The code can be reviewed on <a href="https://github.com/mindleaving/mensch-id" target="_blank" rel="noreferrer">Github</a></small></p>
            </AccordionCard>
            <AccordionCard
                title="What is a challenge?"
                eventKey='faq-q2'
            >
                <p>A challenge is a human-friendly, low-security method of verifying that someone owns a particular 웃ID.</p>
                <p>This is necessary because a 웃ID is not a secret (you can find mine on the contact page) and it is easy to (maliciously) state a wrong ID and thereby impersonating others.</p>
                <p>The process is:</p>
                <ol>
                    <li>Visit the <Link to="/challenge">challenge page</Link></li>
                    <li>Enter the 웃ID you want to verify</li>
                    <li>Give the <strong>challenge ID</strong> to the challenged person. <span className='red'>Keep the secret to yourself!</span></li>
                    <li>The challenged person opens their <Link to="/challenges">challenge page</Link> and search for the challenge ID</li>
                    <li>The challenged person submits/reads the secret to the challenger</li>
                    <li>If the secrets match, the challenged person has proven to be in posession of the 웃ID</li>
                </ol>
            </AccordionCard>
            <AccordionCard
                title="Why do I need this?"
                eventKey='faq-q3'
            >
                <p>An ID is necessary to be able to uniquely identify a person across different contexts.</p>
                <p>
                    A common example would be the transfer of medical data between a hospital and your general practitioner. 
                    To ensure that the hospital only sends the information belonging to you and not from someone with a similar name, a unique ID is needed. 
                    This is already done more or less elegantly in different countries, sometimes using insurance numbers, sometimes using the name and birth date or some other method.
                </p>
                <p>
                    But today we travel a lot and that's where 웃ID tries to step in and be that unique ID. 
                    That's why you need 웃ID and declare it when in contact with health care or public services.
                </p>
            </AccordionCard>
            <AccordionCard
                title="How widely accepted is 웃ID?"
                eventKey='faq-q4'
            >
                <p>웃ID has been released in September 2022 and is currently being proposed to different German, Danish and European authorities and projects.</p>
                {/* <p>A <Link to="/pilot-project-hd">pilot project is planned to take place in Heidelberg, Germany</Link> starting in the first half of 2023.</p> */}
                <p>Even though it is not widely used yet, I would appreciate if you could spread the word and give your 웃ID to your doctor and authorities. Please explain to them that this is a unique ID and point them to this website.</p>
            </AccordionCard>
        </Accordion>
    );

}