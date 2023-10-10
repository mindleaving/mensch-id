import { Row, Col } from "react-bootstrap";
import { Models } from "../../types/models";

interface AddressViewerProps {
    address: Models.Address;
}

export const AddressViewer = (props: AddressViewerProps) => {

    const { address } = props;

    return (
    <>
        <Row>
            <Col>
                {address.street}
            </Col>
        </Row>
        <Row>
            <Col>
                {address.postalCode} {address.city}
            </Col>
        </Row>
        <Row>
            <Col>
                {address.country}
            </Col>
        </Row>
    </>);

}