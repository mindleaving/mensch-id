import { Nav, NavDropdown } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";

interface AdminMenuProps {}

export const AdminMenu = (props: AdminMenuProps) => {

    const navigate = useNavigate();

    return (
    <Nav>
        <NavDropdown title={resolveText("Menu_Actions")}>
            <NavDropdown.Item onClick={() => navigate("/challenge")}>{resolveText("SendChallenge")}</NavDropdown.Item>
            <NavDropdown.Item onClick={() => navigate("/assigner-account-requests")}>{resolveText("Menu_AssignerAccountRequests")}</NavDropdown.Item>
            <NavDropdown.Item onClick={() => navigate("/shop-orders")}>{resolveText("Menu_ShopOrders")}</NavDropdown.Item>
        </NavDropdown>
    </Nav>);

}