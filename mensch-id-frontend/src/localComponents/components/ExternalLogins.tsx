import React from 'react';
import { Row, Col, Button } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { LoginProvider } from '../types/enums.d';

interface ExternalLoginsProps {
    availableExternalProviders: LoginProvider[];
    onProviderSelected: (loginProvider: LoginProvider) => void;
}

export const ExternalLogins = (props: ExternalLoginsProps) => {

    if(props.availableExternalProviders.length === 0) {
        return (<Row>
            <Col>{resolveText("ExternalLogin_NoProviders")}</Col>
        </Row>)
    }

    return (
        <Row>
            {props.availableExternalProviders.includes(LoginProvider.Google)
            ? <Col xs="auto">
                <Button
                    size="lg"
                    variant="outline-dark"
                    onClick={() => props.onProviderSelected(LoginProvider.Google)}
                >
                    <i className='fa fa-google' />
                </Button>
            </Col>: null}
            {props.availableExternalProviders.includes(LoginProvider.Twitter)
            ? <Col xs="auto">
                <Button
                    size="lg"
                    variant="outline-dark"
                    onClick={() => props.onProviderSelected(LoginProvider.Twitter)}
                >
                    <i className='fa fa-twitter' />
                </Button>
            </Col>: null}
            {props.availableExternalProviders.includes(LoginProvider.Microsoft)
            ? <Col xs="auto">
                <Button
                    size="lg"
                    variant="outline-dark"
                    onClick={() => props.onProviderSelected(LoginProvider.Microsoft)}
                >
                    <i className='fa fa-windows' />
                </Button>
            </Col>: null}
            {props.availableExternalProviders.includes(LoginProvider.Facebook)
            ? <Col xs="auto">
                <Button
                    size="lg"
                    variant="outline-dark"
                    onClick={() => props.onProviderSelected(LoginProvider.Facebook)}
                >
                    <i className='fa fa-facebook' />
                </Button>
            </Col>: null}
            <Col />
        </Row>
    );

}