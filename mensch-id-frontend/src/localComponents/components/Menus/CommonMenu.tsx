import { Nav, NavDropdown } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";

interface CommonMenuProps {}

export const CommonMenu = (props: CommonMenuProps) => {

    const navigate = useNavigate();

    return (
    <Nav>
        <NavDropdown title={resolveText("Menu_About")}>
            <NavDropdown.Item onClick={() => navigate("/about")}>{resolveText("Menu_FAQ")}</NavDropdown.Item>
            <NavDropdown.Item onClick={() => navigate("/privacy")}>{resolveText("Menu_Privacy")}</NavDropdown.Item>
            <NavDropdown.Item onClick={() => navigate("/terms-of-service")}>{resolveText("Menu_TermsOfService")}</NavDropdown.Item>
            {/* <NavDropdown.Item onClick={() => navigate("/pilot-project-hd")}>{resolveText("PilotProject_Heidelberg")}</NavDropdown.Item> */}
            <NavDropdown.Item onClick={() => navigate("/request-assigner-account")}>{resolveText("Menu_BecomeAnAssigner")}</NavDropdown.Item>
            <NavDropdown.Item onClick={() => navigate("/contact")}>{resolveText("Impressum")}</NavDropdown.Item>
        </NavDropdown>
    </Nav>
    );

}