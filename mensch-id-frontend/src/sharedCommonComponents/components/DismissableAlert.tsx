import { PropsWithChildren } from "react";
import { Alert, CloseButton, Row, Col } from "react-bootstrap";

interface DismissableAlertProps extends PropsWithChildren {
    variant?: "primary" | "secondary" | "info" | "success" | "warning" | "danger";
    isDismissable?: boolean;
    onDismiss?: () => void;
}

export const DismissableAlert = (props: DismissableAlertProps) => {

    const { variant, isDismissable, onDismiss } = props;

    return (<Alert 
        variant={variant}
        className='py-2 my-0'
    >
        <Row className='align-items-center'>
            <Col>
                {props.children}
            </Col>
            {isDismissable
            ? <Col xs="auto">
                <CloseButton onClick={onDismiss} />
            </Col> : null}
        </Row>
    </Alert>);

}