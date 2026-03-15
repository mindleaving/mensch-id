import React, { PropsWithChildren, ReactNode, useEffect, useState } from 'react';
import { Accordion, Button, Card, Col, Row } from 'react-bootstrap';
import '../styles/accordion-card.css';
import { combineCssClasses } from '../helpers/StylingHelpers';

interface AccordionCardProps extends PropsWithChildren<{}> {
    eventKey: string;
    title: ReactNode;
    collapsedTitle?: ReactNode;
    id?: string;
    className?: string;
    headerClassName?: string;
    bodyClassName?: string;
    bg?: string;
    standalone?: boolean;
    isOpenAtCreate?: boolean;
    mountOnEnter?: boolean;
    onMount?: () => void;
    hasMoveUpButton?: boolean;
    onMoveUp?: () => void;
    hasMoveDownButton?: boolean;
    onMoveDown?: () => void;
}

export const AccordionCard = (props: AccordionCardProps) => {

    const [ hasBeenMounted, setHasBeenMounted ] = useState<boolean>(!props.mountOnEnter || (props.isOpenAtCreate ?? false));
    const [ isCollapsed, setIsCollapsed ] = useState<boolean>(!props.isOpenAtCreate);

    useEffect(() => {
        if(!isCollapsed) {
            if(!hasBeenMounted) {
                setHasBeenMounted(true);
            }
        }
    }, [ isCollapsed ]);

    useEffect(() => {
        if(hasBeenMounted) {
            if(props.onMount) {
                props.onMount();
            }
        }
    }, [ hasBeenMounted ]);

    const accordionItem = (
        <Accordion.Item
            id={props.id}
            className={props.className}
            eventKey={props.eventKey}
        >
            <Accordion.Button 
                as="div"
                className={combineCssClasses([
                    props.headerClassName,
                    props.hasMoveUpButton || props.hasMoveDownButton ? `clickable py-2` : 'clickable'
                ])}
                bsPrefix={`accordion-button` + (props.bg ? ` bg-${props.bg}` : '')}
            >
                <Row className="align-items-center w-100 pe-3">
                    <Col>
                        {isCollapsed && props.collapsedTitle ? props.collapsedTitle : props.title}
                    </Col>
                    {props.hasMoveUpButton
                    ? <Col xs="auto" className='px-1'>
                        <Button
                            onClick={e => {
                                e.stopPropagation();
                                if(props.onMoveUp) {
                                    props.onMoveUp();
                                }
                            }}
                            variant="outline-primary"
                        >
                            <i className="fa fa-arrow-up" />
                        </Button>
                    </Col> : null}
                    {props.hasMoveDownButton
                    ? <Col xs="auto" className='px-1'>
                        <Button
                            onClick={e => {
                                e.stopPropagation();
                                if(props.onMoveDown) {
                                    props.onMoveDown();
                                }
                            }}
                            variant="outline-primary"
                        >
                            <i className="fa fa-arrow-down" />
                        </Button>
                    </Col> : null}
                </Row>
            </Accordion.Button>
            <Accordion.Collapse 
                eventKey={props.eventKey}
                onEnter={() => setIsCollapsed(false)}
                onExited={() => setIsCollapsed(true)}
            >
                <Card.Body className={props.bodyClassName ?? 'py-2 px-3'}>
                    {hasBeenMounted ? props.children : null}
                </Card.Body>
            </Accordion.Collapse>
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