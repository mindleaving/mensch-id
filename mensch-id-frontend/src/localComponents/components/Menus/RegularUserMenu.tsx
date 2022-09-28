import React from 'react';
import { Nav, NavDropdown } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';

interface RegularUserMenuProps {}

export const RegularUserMenu = (props: RegularUserMenuProps) => {

    const navigate = useNavigate();
    return (
        <Nav>
            <Nav.Link onClick={() => navigate("/me")}>{resolveText("Menu_MyProfile")}</Nav.Link>
            <NavDropdown title={resolveText("Menu_Actions")}>
                <NavDropdown.Item onClick={() => navigate("/linkaccount")}>
                    {resolveText("Menu_LinkToAnotherAccount")}
                </NavDropdown.Item>
                <NavDropdown.Item onClick={() => navigate("/challenges")}>
                    {resolveText("Menu_MyChallenges")}
                </NavDropdown.Item>
                <NavDropdown.Item onClick={() => navigate("/accounts")}>
                    {resolveText("Menu_ManageAccounts")}
                </NavDropdown.Item>
            </NavDropdown>
        </Nav>
    );

}