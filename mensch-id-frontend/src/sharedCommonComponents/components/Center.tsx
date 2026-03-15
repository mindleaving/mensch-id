import React, { CSSProperties, PropsWithChildren } from 'react';
import { Row, Col } from 'react-bootstrap';

interface CenterProps {
    className?: string,
    style?: CSSProperties;
}

export const Center = (props: PropsWithChildren<CenterProps>) => {

    return (
        <Row className={props.className} style={props.style}>
            <Col />
            <Col xs="auto">{props.children}</Col>
            <Col />
        </Row>
    );

}