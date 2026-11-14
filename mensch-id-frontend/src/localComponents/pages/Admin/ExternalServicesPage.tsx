import { useCallback, useEffect, useState } from 'react';
import { Badge, Button, Col, FormSelect, Row, Table } from 'react-bootstrap';
import { AsyncButton } from '../../../sharedCommonComponents/components/AsyncButton';
import { LoadingTableRow } from '../../../sharedCommonComponents/components/LoadingTableRow';
import { NoEntriesTableRow } from '../../../sharedCommonComponents/components/NoEntriesTableRow';
import { openConfirmAlert, showSuccessAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { deleteObject } from '../../../sharedCommonComponents/helpers/DeleteHelpers';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import { buildLoadObjectFunc } from '../../../sharedCommonComponents/helpers/LoadingHelpers';
import { sendPostRequest } from '../../../sharedCommonComponents/helpers/StoringHelpers';
import { DelayedFormControl } from '../../components/DelayedFormControl';
import { formatDateTime } from '../../helpers/Formatter';
import { AnyStatusOption, buildFilterQueryParameters } from '../../helpers/FilterHelpers';
import { ExternalServiceDetailsModal } from '../../modals/ExternalServiceDetailsModal';
import { ExternalServiceStatus } from '../../types/enums';
import { Models } from '../../types/models';

interface ExternalServicesPageProps {}

export const ExternalServicesPage = (props: ExternalServicesPageProps) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ services, setServices ] = useState<Models.ExternalServices.ExternalService[]>([]);
    const [ searchText, setSearchText ] = useState<string>('');
    const [ status, setStatus ] = useState<string>(AnyStatusOption);
    const [ submittingServiceId, setSubmittingServiceId ] = useState<string>();
    const [ selectedService, setSelectedService ] = useState<Models.ExternalServices.ExternalService>();

    const loadServices = useCallback(() => {
        const load = buildLoadObjectFunc<Models.ExternalServices.ExternalService[]>(
            'api/externalServices',
            buildFilterQueryParameters(searchText, status),
            resolveText("ExternalServices_CouldNotLoad"),
            setServices,
            undefined,
            () => setIsLoading(false)
        );
        load();
    }, [ searchText, status ]);

    useEffect(() => {
        loadServices();
    }, [ loadServices ]);

    const runAction = async (serviceId: string, action: string, errorText: string, successText: string) => {
        setSubmittingServiceId(serviceId);
        await sendPostRequest(
            `api/externalServices/${serviceId}/${action}`, {},
            errorText,
            null,
            async () => {
                showSuccessAlert(successText);
                loadServices();
            },
            undefined,
            () => setSubmittingServiceId(undefined)
        );
    };

    const accept = (service: Models.ExternalServices.ExternalService) => {
        runAction(service.id, 'activate', resolveText("ExternalService_CouldNotAccept"), resolveText("ExternalService_Accepted"));
    };

    const reject = async (service: Models.ExternalServices.ExternalService) => {
        setSubmittingServiceId(service.id);
        await deleteObject(
            `api/externalServices/${service.id}`, {},
            resolveText("ExternalService_CouldNotReject"),
            () => {
                showSuccessAlert(resolveText("ExternalService_Rejected"));
                setServices(state => state.filter(x => x.id !== service.id));
            },
            undefined,
            () => setSubmittingServiceId(undefined)
        );
    };

    const revoke = (service: Models.ExternalServices.ExternalService) => {
        runAction(service.id, 'deactivate', resolveText("ExternalService_CouldNotRevoke"), resolveText("ExternalService_Revoked"));
    };

    const confirm = (titleKey: string, messageKey: string, service: Models.ExternalServices.ExternalService, action: () => void) => {
        openConfirmAlert({
            title: resolveText(titleKey),
            message: resolveText(messageKey).replace('{0}', service.displayName),
            closeOnClickOutside: true,
            buttons: [
                { label: resolveText("Cancel") },
                { label: resolveText("Confirm"), onClick: action }
            ]
        });
    };

    const renderActions = (service: Models.ExternalServices.ExternalService) => {
        const isSubmitting = submittingServiceId === service.id;
        if(service.status === ExternalServiceStatus.Pending) {
            return (<div className="d-flex gap-2 justify-content-end">
                <AsyncButton
                    variant="success"
                    isExecuting={isSubmitting}
                    disabled={!service.isEmailVerified}
                    title={service.isEmailVerified ? undefined : resolveText("ExternalService_AcceptRequiresVerifiedEmail")}
                    onClick={() => accept(service)}
                    activeText={<><i className="fa fa-check" /> {resolveText("Accept")}</>}
                    executingText={resolveText("Submitting...")}
                />
                <AsyncButton
                    variant="danger"
                    isExecuting={isSubmitting}
                    onClick={() => confirm("ExternalService_ConfirmReject_Title", "ExternalService_ConfirmReject_Message", service, () => reject(service))}
                    activeText={<><i className="fa fa-times" /> {resolveText("Reject")}</>}
                    executingText={resolveText("Submitting...")}
                />
            </div>);
        }
        if(service.status === ExternalServiceStatus.Active) {
            return (<div className="d-flex justify-content-end">
                <AsyncButton
                    variant="danger"
                    isExecuting={isSubmitting}
                    onClick={() => confirm("ExternalService_ConfirmRevoke_Title", "ExternalService_ConfirmRevoke_Message", service, () => revoke(service))}
                    activeText={<><i className="fa fa-ban" /> {resolveText("ExternalService_Revoke")}</>}
                    executingText={resolveText("Submitting...")}
                />
            </div>);
        }
        return null;
    };

    return (
    <>
        <h1>{resolveText("ExternalServices")}</h1>
        <Row className="mb-3 g-2">
            <Col md={8}>
                <DelayedFormControl
                    value={searchText}
                    onChange={setSearchText}
                    placeholder={resolveText("ExternalServices_SearchPlaceholder")}
                />
            </Col>
            <Col md={4}>
                <FormSelect value={status} onChange={e => setStatus(e.target.value)}>
                    <option value={AnyStatusOption}>{resolveText("Any")}</option>
                    {Object.values(ExternalServiceStatus).map(x => (
                        <option key={x} value={x}>{resolveText(`ExternalServiceStatus_${x}`)}</option>
                    ))}
                </FormSelect>
            </Col>
        </Row>
        <Table>
            <thead>
                <tr>
                    <th></th>
                    <th>{resolveText("ExternalService_DisplayName")}</th>
                    <th>{resolveText("ExternalService_Email")}</th>
                    <th>{resolveText("ExternalService_Status")}</th>
                    <th>{resolveText("ExternalService_Created")}</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                {isLoading ? <LoadingTableRow colSpan={6} />
                : services.length === 0 ? <NoEntriesTableRow colSpan={6} />
                : services.map(service => (
                    <tr key={service.id}>
                        <td>
                            <Button variant="outline-secondary" size="sm" onClick={() => setSelectedService(service)}>
                                <i className="fa fa-info-circle" /> {resolveText("Details")}
                            </Button>
                        </td>
                        <td>{service.displayName}</td>
                        <td>
                            {service.email}
                            {!service.isEmailVerified
                            ? <Badge pill bg="light" text="danger" className="ms-2">{resolveText("ExternalService_EmailNotVerified")}</Badge>
                            : null}
                        </td>
                        <td>{resolveText(`ExternalServiceStatus_${service.status}`)}</td>
                        <td>{formatDateTime(new Date(service.createdTimestamp))}</td>
                        <td>{renderActions(service)}</td>
                    </tr>
                ))}
            </tbody>
        </Table>
        {selectedService
        ? <ExternalServiceDetailsModal
            show
            service={selectedService}
            onClose={() => setSelectedService(undefined)}
        />
        : null}
    </>);

};
export default ExternalServicesPage;
