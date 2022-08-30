import React from 'react';
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
                <NavDropdown.Item onClick={() => navigate("/challenge")}>
                    {resolveText("SendChallenge")}
                </NavDropdown.Item>
            </NavDropdown>
        </Nav>
    );

}