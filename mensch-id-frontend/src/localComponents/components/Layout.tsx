import React, { PropsWithChildren } from 'react';
import { Col, Container, Row } from 'react-bootstrap';

interface LayoutProps {}

export const Layout = (props: PropsWithChildren<LayoutProps>) => {

    return (
        <Container>
            <Row>
                <Col>
                    {props.children}
                </Col>
            </Row>
        </Container>
    );

}