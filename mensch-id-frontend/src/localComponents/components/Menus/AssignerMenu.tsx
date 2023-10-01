import { Nav, NavDropdown } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';

interface AssignerMenuProps {}

export const AssignerMenu = (props: AssignerMenuProps) => {

    const navigate = useNavigate();
    return (
        <Nav>
            <Nav.Link onClick={() => navigate("/assigner")}>{resolveText("Assigner")}</Nav.Link>
            <NavDropdown title={resolveText("Menu_Actions")}>
                <NavDropdown.Item onClick={() => navigate("/challenge")}>{resolveText("SendChallenge")}</NavDropdown.Item>
                <NavDropdown.Item onClick={() => navigate("/account")}>{resolveText("Menu_ManageAccount")}</NavDropdown.Item>
                <NavDropdown.Item onClick={() => navigate("/shop")}>{resolveText("Menu_Shop")}</NavDropdown.Item>
                <NavDropdown.Item onClick={() => navigate("/shop/orders")}>{resolveText("Menu_ShopOrders")}</NavDropdown.Item>
            </NavDropdown>
        </Nav>
    );

}