import { Badge, Modal, Table } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { formatDateTime } from '../helpers/Formatter';
import { Models } from '../types/models';

interface ExternalServiceDetailsModalProps {
    show: boolean;
    onClose: () => void;
    service: Models.ExternalServices.ExternalService;
}

export const ExternalServiceDetailsModal = (props: ExternalServiceDetailsModalProps) => {

    const { show, onClose, service } = props;

    return (
    <Modal show={show} onHide={onClose} size="lg">
        <Modal.Header closeButton>
            <Modal.Title>{service.displayName}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
            <Table>
                <tbody>
                    <tr>
                        <th>{resolveText("ExternalService_Status")}</th>
                        <td>{resolveText(`ExternalServiceStatus_${service.status}`)}</td>
                    </tr>
                    <tr>
                        <th>{resolveText("ExternalService_Email")}</th>
                        <td>
                            <a href={`mailto:${service.email}`}>{service.email}</a>
                            <Badge pill bg="light" text={service.isEmailVerified ? 'success' : 'danger'} className="ms-2">
                                {resolveText(service.isEmailVerified ? "Verified" : "ExternalService_EmailNotVerified")}
                            </Badge>
                        </td>
                    </tr>
                    <tr>
                        <th>{resolveText("ExternalService_ContactInformation")}</th>
                        <td style={{ whiteSpace: 'pre-line' }}>{service.contactInformation}</td>
                    </tr>
                    <tr>
                        <th>{resolveText("ExternalService_ClientId")}</th>
                        <td><code>{service.clientId}</code></td>
                    </tr>
                    <tr>
                        <th>{resolveText("ExternalService_Created")}</th>
                        <td>{formatDateTime(new Date(service.createdTimestamp))}</td>
                    </tr>
                    {service.activatedTimestamp
                    ? <tr>
                        <th>{resolveText("ExternalService_Activated")}</th>
                        <td>
                            {formatDateTime(new Date(service.activatedTimestamp))}
                            {service.activatedBy ? <div className="text-secondary"><small>{service.activatedBy}</small></div> : null}
                        </td>
                    </tr>
                    : null}
                </tbody>
            </Table>
        </Modal.Body>
    </Modal>);

};
