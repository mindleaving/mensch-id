import React from 'react';
import { Accordion } from 'react-bootstrap';
import { AccordionCard } from '../../../sharedCommonComponents/components/AccordionCard';

interface FAQ_enProps {}

export const FAQ_en = (props: FAQ_enProps) => {

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
        </Accordion>
    );

}