import { PropsWithChildren, Suspense } from 'react';
import { Button, Container, Nav, Navbar, NavItem } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { AccountType } from '../types/enums';
import { AssignerMenu } from './Menus/AssignerMenu';
import { RegularUserMenu } from './Menus/RegularUserMenu';
import { ToastContainer } from 'react-toastify';
import { CommonMenu } from './Menus/CommonMenu';
import { LoadingAlert } from '../../sharedCommonComponents/components/LoadingAlert';
import { AdminMenu } from './Menus/AdminMenu';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';

interface LayoutProps {
    isLoggedIn: boolean;
    accountType?: AccountType;
    onLogOut: () => void;
}

export const Layout = (props: PropsWithChildren<LayoutProps>) => {

    const { isLoggedIn, accountType, onLogOut } = props;
    const navigate = useNavigate();

    let userMenu = null;
    if(isLoggedIn) {
        switch(accountType!) {
            case AccountType.Admin:
                userMenu = (<AdminMenu />);
                break;
            case AccountType.Assigner:
                userMenu = (<AssignerMenu />);
                break;
            default:
                userMenu = (<RegularUserMenu />);
                break;
        }
    }

    const logOut = async () => {
        await sendPostRequest(
            `api/accounts/logout`, {},
            resolveText('CouldNotLogOut'),
            null,
            onLogOut
        );
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
                        <CommonMenu />
                        <Nav className='me-auto' />
                        {isLoggedIn
                        ? <Navbar.Text>
                            <Button 
                                variant='danger' 
                                onClick={logOut}
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
            <Suspense fallback={<LoadingAlert />}>
                {props.children}
            </Suspense>
        </>
    );

}