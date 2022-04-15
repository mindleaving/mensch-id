import React, { PropsWithChildren } from 'react';
import { Button, Col, Container, Navbar, Row } from 'react-bootstrap';
import { NotificationContainer } from 'react-notifications';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface LayoutProps {
    isLoggedIn: boolean;
    onLogOut: () => void;
}

export const Layout = (props: PropsWithChildren<LayoutProps>) => {

    return (
        <>
            <NotificationContainer />
            {props.isLoggedIn
            ? <Navbar bg="dark" variant='dark' className='mb-3'>
                <Container>
                    <Navbar.Brand>웃ID</Navbar.Brand>
                    <Navbar.Text>
                        <Button variant='danger' onClick={props.onLogOut}>{resolveText("LogOut")}</Button>
                    </Navbar.Text>
                </Container>
            </Navbar>
            : null}
            <Container>
                <Row>
                    <Col>
                        {props.children}
                    </Col>
                </Row>
            </Container>
        </>
    );

}