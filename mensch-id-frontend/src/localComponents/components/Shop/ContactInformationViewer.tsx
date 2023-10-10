import { Col, Row } from "react-bootstrap";
import { Models } from "../../types/models";
import { AddressViewer } from "./AddressViewer";

interface ContactInformationViewerProps {
    contactInformation: Models.Contact;
}

export const ContactInformationViewer = (props: ContactInformationViewerProps) => {

    const { contactInformation } = props;

    return (
    <>
        <Row>
            <Col>
                {contactInformation.name}
            </Col>
        </Row>
        <Row>
            <Col>
                {contactInformation.email}
            </Col>
        </Row>
        <Row>
            <Col>
                {contactInformation.phoneNumber}
            </Col>
        </Row>
        <AddressViewer
            address={contactInformation.address}
        />
    </>);

}