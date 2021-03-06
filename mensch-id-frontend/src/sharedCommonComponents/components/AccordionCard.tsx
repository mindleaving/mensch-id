import React, { PropsWithChildren, ReactNode } from 'react';
import { Accordion } from 'react-bootstrap';
import '../styles/accordion-card.css';

interface AccordionCardProps extends PropsWithChildren<{}> {
    eventKey: string;
    title: ReactNode;
    collapsedTitle?: ReactNode;
    className?: string;
    bg?: string;
    standalone?: boolean;
    isOpenAtCreate?: boolean;
}

export const AccordionCard = (props: AccordionCardProps) => {

    const isCollapsed = false;

    const accordionItem = (
        <Accordion.Item
            className={props.className}
            eventKey={props.eventKey}
        >
            <Accordion.Button bsPrefix={`accordion-button` + (props.bg ? ` bg-${props.bg}` : '')}>
                {isCollapsed && props.collapsedTitle ? props.collapsedTitle : props.title}
            </Accordion.Button>
            <Accordion.Body>
                {props.children}
            </Accordion.Body>
        </Accordion.Item>
    );
    if(!props.standalone) {
        return accordionItem;
    }
    return (
        <Accordion 
            defaultActiveKey={props.isOpenAtCreate ? props.eventKey : undefined}
        >
            {accordionItem}
        </Accordion>
    );

}