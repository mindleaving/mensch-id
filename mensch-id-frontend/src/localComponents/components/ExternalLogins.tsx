import { Row, Col, Button } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { getLoginProviderIconClass } from '../helpers/LoginProviderHelpers';
import { ViewModels } from '../types/viewModels';

interface ExternalLoginsProps {
    availableExternalProviders: ViewModels.LoginProviderViewModel[];
    onProviderSelected: (loginProviderId: string) => void;
}

export const ExternalLogins = (props: ExternalLoginsProps) => {

    if(props.availableExternalProviders.length === 0) {
        return (<Row>
            <Col>{resolveText("ExternalLogin_NoProviders")}</Col>
        </Row>);
    }

    return (
        <Row>
            {props.availableExternalProviders.map(loginProvider => {
                const iconClass = getLoginProviderIconClass(loginProvider.id);
                return (<Col xs="auto" key={loginProvider.id}>
                    <Button
                        size="lg"
                        variant="outline-dark"
                        title={loginProvider.displayName}
                        onClick={() => props.onProviderSelected(loginProvider.id)}
                    >
                        {iconClass
                        ? <i className={iconClass} />
                        : loginProvider.displayName}
                    </Button>
                </Col>);
            })}
            <Col />
        </Row>
    );

};
