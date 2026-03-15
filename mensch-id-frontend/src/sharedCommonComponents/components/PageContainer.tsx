import { CSSProperties, PropsWithChildren } from 'react';
import { Container, Row, Col } from 'react-bootstrap';

interface PageContainerProps {
    className?: string;
    style?: CSSProperties;
}
export const PageContainer = (props: PropsWithChildren<PageContainerProps>) => {
    return (
    <Container style={props.style ?? { maxWidth: '90%' }} className={props.className}>
        <Row>
            <Col>{props.children}</Col>
        </Row>
    </Container>
    );
}
export default PageContainer;