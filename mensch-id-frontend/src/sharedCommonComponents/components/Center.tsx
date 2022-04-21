import React, { PropsWithChildren } from 'react';
import { Row, Col } from 'react-bootstrap';

interface CenterProps {}

export const Center = (props: PropsWithChildren<CenterProps>) => {

    return (
        <Row>
            <Col />
            <Col>{props.children}</Col>
            <Col />
        </Row>
    );

}