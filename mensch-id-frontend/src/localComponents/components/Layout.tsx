import React, { PropsWithChildren } from 'react';
import { Button, Col, Container, Nav, Navbar, NavDropdown, Row } from 'react-bootstrap';
import { NotificationContainer } from 'react-notifications';
import { useNavigate } from 'react-router-dom';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface LayoutProps {
    isLoggedIn: boolean;
    onLogOut: () => void;
}

export const Layout = (props: PropsWithChildren<LayoutProps>) => {


    const navigate = useNavigate();

    return (
        <>
            <NotificationContainer />
            <Navbar expand="lg" bg="dark" variant='dark' className='mb-3'>
                <Container>
                    <Navbar.Brand className='clickable' onClick={() => navigate("/")}>웃ID</Navbar.Brand>
                    <Navbar.Toggle />
                    <Navbar.Collapse>
                        <Nav>
                            {props.isLoggedIn
                            ? <>
                                <Nav.Link onClick={() => navigate("/me")}>{resolveText("Menu_MyProfile")}</Nav.Link>
                                <NavDropdown title={resolveText("Menu_Actions")}>
                                    <NavDropdown.Item onClick={() => navigate("/linkaccount")}>
                                        {resolveText("Menu_LinkToAnotherAccount")}
                                    </NavDropdown.Item>
                                    <NavDropdown.Item onClick={() => navigate("/challenges")}>
                                        {resolveText("Menu_MyChallenges")}
                                    </NavDropdown.Item>
                                </NavDropdown>
                            </>
                            : null}
                            <Nav.Link onClick={() => navigate("/privacy")}>{resolveText("Menu_Privacy")}</Nav.Link>
                            <Nav.Link onClick={() => navigate("/terms-of-service")}>{resolveText("Menu_TermsOfService")}</Nav.Link>
                        </Nav>
                        <Nav className='me-auto' />
                        {props.isLoggedIn
                        ? <Navbar.Text>
                            <Button 
                                variant='danger' 
                                onClick={props.onLogOut}
                            >
                                {resolveText("LogOut")}
                            </Button>
                        </Navbar.Text> 
                        : <Navbar.Text>
                            <Button 
                                variant='primary'
                                onClick={() => navigate("/login")}
                            >
                                {resolveText("Login")}
                            </Button>
                        </Navbar.Text>}
                    </Navbar.Collapse>
                </Container>
            </Navbar>
            <Container style={{ marginBottom: '100px'}}>
                <Row>
                    <Col>
                        {props.children}
                    </Col>
                </Row>
            </Container>
            <Navbar bg="dark" variant="dark" fixed='bottom'>
                <Container>
                    <Nav className='ms-auto me-auto'>
                        <Nav.Link onClick={() => navigate("/contact")}>Impressum</Nav.Link>
                    </Nav>
                </Container>
            </Navbar>
        </>
    );

}