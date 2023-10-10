import { PropsWithChildren } from "react";
import { Container, Row, Col } from "react-bootstrap";

interface PageContainerProps {}

export const PageContainer = (props: PropsWithChildren<PageContainerProps>) => {

    return (
    <Container style={{ marginBottom: '100px'}}>
        <Row>
            <Col>
                {props.children}
            </Col>
        </Row>
    </Container>
    );

}