import { Badge, Card, Modal, Table } from 'react-bootstrap';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { ContactInformationViewer } from '../components/Shop/ContactInformationViewer';
import { ViewModels } from '../types/viewModels';

interface AssignerAccountDetailsModalProps {
    show: boolean;
    onClose: () => void;
    assigner: ViewModels.AdminAssignerViewModel;
}

export const AssignerAccountDetailsModal = (props: AssignerAccountDetailsModalProps) => {

    const { show, onClose, assigner } = props;

    return (
    <Modal show={show} onHide={onClose} size="lg">
        <Modal.Header closeButton>
            <Modal.Title>{assigner.name}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
            {assigner.logoId
            ? <div className="text-center mb-3">
                <img
                    src={apiClient.instance!.buildUrl(`/api/assigner/${assigner.id}/logo/${assigner.logoId}`)}
                    alt={assigner.name}
                    style={{ maxHeight: 120, maxWidth: '100%' }}
                />
            </div>
            : null}
            <Table>
                <tbody>
                    <tr>
                        <th>{resolveText("AssignerAccount_Username")}</th>
                        <td>{assigner.username}</td>
                    </tr>
                    <tr>
                        <th>{resolveText("AssignerAccount_Status")}</th>
                        <td>
                            <Badge bg={assigner.isActivated ? 'success' : 'secondary'}>
                                {resolveText(assigner.isActivated ? "AssignerAccountStatus_Activated" : "AssignerAccountStatus_Deactivated")}
                            </Badge>
                        </td>
                    </tr>
                    <tr>
                        <th>{resolveText("AssignerAccount_AssignedProfileCount")}</th>
                        <td>{assigner.assignedProfileCount}</td>
                    </tr>
                </tbody>
            </Table>
            {assigner.contact
            ? <Card>
                <Card.Header>
                    <Card.Title>{resolveText("ContactInformation")}</Card.Title>
                </Card.Header>
                <Card.Body>
                    <ContactInformationViewer contactInformation={assigner.contact} />
                </Card.Body>
            </Card>
            : null}
        </Modal.Body>
    </Modal>);

};
