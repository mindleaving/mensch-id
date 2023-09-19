import { Col, Row } from "react-bootstrap";

interface HorizontalLineWithTextProps {
    text: string;
    variant?: string;
}

export const HorizontalLineWithText = (props: HorizontalLineWithTextProps) => {

    return (
        <Row className="align-items-center my-1">
            <Col className="pe-0"><div className="horizontal-line" /></Col>
            <Col xs="auto"><span className={props.variant ? `text-${props.variant}` : undefined}>{props.text}</span></Col>
            <Col className="ps-0"><div className="horizontal-line" /></Col>
        </Row>
    );

}