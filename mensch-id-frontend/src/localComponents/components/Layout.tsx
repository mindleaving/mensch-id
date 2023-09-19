import React, { PropsWithChildren } from 'react';
import { Button, Col, Container, Nav, Navbar, NavDropdown, NavItem, Row } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { AccountType } from '../types/enums.d';
import { AssignerMenu } from './Menus/AssignerMenu';
import { RegularUserMenu } from './Menus/RegularUserMenu';
import { ToastContainer } from 'react-toastify';

interface LayoutProps {
    isLoggedIn: boolean;
    accountType?: AccountType;
    onLogOut: () => void;
}

export const Layout = (props: PropsWithChildren<LayoutProps>) => {

    const navigate = useNavigate();

    let userMenu = null;
    if(props.isLoggedIn) {
        switch(props.accountType!) {
            case AccountType.Assigner:
                userMenu = (<AssignerMenu />);
                break;
            default:
                userMenu = (<RegularUserMenu />);
                break;
        }
    }

    return (
        <>
            <ToastContainer 
                theme='colored'
            />
            {window.location.hostname === 'localhost' || window.location.hostname.startsWith('test.')
            ? <Navbar bg='warning' className='justify-content-center'>
                <Nav>
                    <NavItem>This is a test system</NavItem>
                </Nav>
            </Navbar> : null}
            <Navbar expand="lg" bg="dark" variant='dark' className='mb-3 no-print'>
                <Container>
                    <Navbar.Brand className='clickable' onClick={() => navigate("/")}>웃ID</Navbar.Brand>
                    <Navbar.Toggle />
                    <Navbar.Collapse>
                        {userMenu}
                        <Nav>
                            <Nav.Link onClick={() => navigate("/privacy")}>{resolveText("Menu_Privacy")}</Nav.Link>
                            <Nav.Link onClick={() => navigate("/terms-of-service")}>{resolveText("Menu_TermsOfService")}</Nav.Link>
                            <NavDropdown title={resolveText("Menu_About")}>
                                <NavDropdown.Item onClick={() => navigate("/about")}>{resolveText("Menu_FAQ")}</NavDropdown.Item>
                                {/* <NavDropdown.Item onClick={() => navigate("/pilot-project-hd")}>{resolveText("PilotProject_Heidelberg")}</NavDropdown.Item> */}
                                <NavDropdown.Item onClick={() => navigate("/contact")}>{resolveText("Impressum")}</NavDropdown.Item>
                            </NavDropdown>
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
        </>
    );

}