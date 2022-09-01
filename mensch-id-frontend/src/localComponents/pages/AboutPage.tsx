import React from 'react';
import { Accordion } from 'react-bootstrap';
import { AccordionCard } from '../../sharedCommonComponents/components/AccordionCard';
import { VerbatimText } from '../../sharedCommonComponents/components/VerbatimText';
import { canResolveText, resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface AboutPageProps {}

export const AboutPage = (props: AboutPageProps) => {

    const faqCards = [];
    let faqIndex = 1;
    while(canResolveText(`FAQ_Q${faqIndex}`)) {
        const faqCard = (
            <AccordionCard
                title={resolveText(`FAQ_Q${faqIndex}`)}
                eventKey={`faq-q${faqIndex}`}
            >
                <VerbatimText
                    text={resolveText(`FAQ_A${faqIndex}`)}
                />
            </AccordionCard>
        );
        faqCards.push(faqCard);
    }

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
            <Accordion>
                {faqCards}
            </Accordion>
        </>
    );

}