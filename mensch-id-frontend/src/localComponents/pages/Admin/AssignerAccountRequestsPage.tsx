import { useCallback, useEffect, useState } from "react";
import { Badge, Button, Col, FormSelect, Row, Table } from "react-bootstrap";
import { LoadingTableRow } from "../../../sharedCommonComponents/components/LoadingTableRow";
import { NoEntriesTableRow } from "../../../sharedCommonComponents/components/NoEntriesTableRow";
import { openConfirmAlert, openConfirmDeleteAlert, showSuccessAlert } from "../../../sharedCommonComponents/helpers/AlertHelpers";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { buildLoadObjectFunc } from "../../../sharedCommonComponents/helpers/LoadingHelpers";
import { sendPostRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { Models } from "../../types/models";
import { ViewModels } from "../../types/viewModels";
import { deleteObject } from "../../../sharedCommonComponents/helpers/DeleteHelpers";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";
import { DelayedFormControl } from "../../components/DelayedFormControl";
import { AnyStatusOption, buildFilterQueryParameters } from "../../helpers/FilterHelpers";
import { AssignerAccountDetailsModal } from "../../modals/AssignerAccountDetailsModal";
import { AssignerAccountStatus } from "../../types/enums";

const RequestedStatusOption = 'Requested';

interface AssignerAccountRequestsPageProps {}

export const AssignerAccountRequestsPage = (props: AssignerAccountRequestsPageProps) => {

    const [ isLoadingRequests, setIsLoadingRequests ] = useState<boolean>(true);
    const [ isLoadingAssigners, setIsLoadingAssigners ] = useState<boolean>(true);
    const [ requests, setRequests ] = useState<Models.AssignerAccountRequest[]>([]);
    const [ assigners, setAssigners ] = useState<ViewModels.AdminAssignerViewModel[]>([]);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ submittingAssignerId, setSubmittingAssignerId ] = useState<string>();
    const [ selectedAssigner, setSelectedAssigner ] = useState<ViewModels.AdminAssignerViewModel>();
    const [ searchText, setSearchText ] = useState<string>('');
    const [ status, setStatus ] = useState<string>(AnyStatusOption);
    const isRequestsSectionVisible = status === AnyStatusOption || status === RequestedStatusOption;
    const isAssignersSectionVisible = status !== RequestedStatusOption;

    const loadRequests = useCallback(() => {
        const load = buildLoadObjectFunc<Models.AssignerAccountRequest[]>(
            'api/admin/assigner-requests',
            buildFilterQueryParameters(searchText, AnyStatusOption),
            resolveText("AssignerAccountRequests_CouldNotLoad"),
            setRequests,
            undefined,
            () => setIsLoadingRequests(false)
        );
        load();
    }, [ searchText ]);

    const loadAssigners = useCallback(() => {
        const assignerStatus = status === RequestedStatusOption ? AnyStatusOption : status;
        const load = buildLoadObjectFunc<ViewModels.AdminAssignerViewModel[]>(
            'api/admin/assigners',
            buildFilterQueryParameters(searchText, assignerStatus),
            resolveText("AssignerAccounts_CouldNotLoad"),
            setAssigners,
            undefined,
            () => setIsLoadingAssigners(false)
        );
        load();
    }, [ searchText, status ]);

    useEffect(() => {
        if(isRequestsSectionVisible) {
            loadRequests();
        }
    }, [ loadRequests, isRequestsSectionVisible ]);

    useEffect(() => {
        if(isAssignersSectionVisible) {
            loadAssigners();
        }
    }, [ loadAssigners, isAssignersSectionVisible ]);

    const reject = async (id: string, force: boolean = false) => {
        if(!force) {
            const request = requests.find(x => x.id === id);
            if(!request) {
                return;
            }
            openConfirmDeleteAlert(
                request.contactPersonName,
                resolveText("AssignerAccountRequest_ConfirmDelete_Title"),
                resolveText("AssignerAccountRequest_ConfirmDelete_Message"),
                () => reject(id, true)
            );
            return;
        }
        setIsSubmitting(true);
        await deleteObject(
            `api/admin/assigner-requests/${id}`, {},
            resolveText("AssignerAccountRequest_CouldNotDelete"),
            () => {
                showSuccessAlert(resolveText("AssignerAccountRequest_SuccessfullyDeleted"));
                setRequests(state => state.filter(x => x.id !== id));
            },
            undefined,
            () => setIsSubmitting(false)
        );
    };

    const accept = async (id: string) => {
        setIsSubmitting(true);
        await sendPostRequest(
            `api/admin/assigner-requests/${id}/approve`, {},
            resolveText("AssignerAccountRequest_CouldNotApprove"),
            null,
            async () => {
                showSuccessAlert(resolveText("AssignerAccountRequest_SuccessfullyApproved"));
                setRequests(state => state.filter(x => x.id !== id));
                loadAssigners();
            },
            undefined,
            () => setIsSubmitting(false)
        );
    };

    const changeAssignerActivation = async (assigner: ViewModels.AdminAssignerViewModel, isActivated: boolean) => {
        setSubmittingAssignerId(assigner.id);
        await sendPostRequest(
            `api/admin/assigners/${assigner.id}/${isActivated ? 'activate' : 'deactivate'}`, {},
            resolveText(isActivated ? "AssignerAccount_CouldNotActivate" : "AssignerAccount_CouldNotDeactivate"),
            null,
            async () => {
                showSuccessAlert(resolveText(isActivated ? "AssignerAccount_Activated" : "AssignerAccount_Deactivated"));
                setAssigners(state => state.map(x => x.id === assigner.id ? { ...x, isActivated: isActivated } : x));
            },
            undefined,
            () => setSubmittingAssignerId(undefined)
        );
    };

    const confirmDeactivation = (assigner: ViewModels.AdminAssignerViewModel) => {
        openConfirmAlert({
            title: resolveText("AssignerAccount_ConfirmDeactivate_Title"),
            message: resolveText("AssignerAccount_ConfirmDeactivate_Message").replace('{0}', assigner.name),
            closeOnClickOutside: true,
            buttons: [
                { label: resolveText("Cancel") },
                { label: resolveText("AssignerAccount_Deactivate"), onClick: () => changeAssignerActivation(assigner, false) }
            ]
        });
    };

    return (
    <>
        <h1>{resolveText("AssignerAccountRequests")}</h1>
        <Row className="mb-3 g-2">
            <Col md={8}>
                <DelayedFormControl
                    value={searchText}
                    onChange={setSearchText}
                    placeholder={resolveText("AssignerAccounts_SearchPlaceholder")}
                />
            </Col>
            <Col md={4}>
                <FormSelect value={status} onChange={e => setStatus(e.target.value)}>
                    <option value={AnyStatusOption}>{resolveText("Any")}</option>
                    <option value={RequestedStatusOption}>{resolveText("AssignerAccountStatus_Requested")}</option>
                    {Object.values(AssignerAccountStatus).map(x => (
                        <option key={x} value={x}>{resolveText(`AssignerAccountStatus_${x}`)}</option>
                    ))}
                </FormSelect>
            </Col>
        </Row>

        {isRequestsSectionVisible
        ? <>
            <h3>{resolveText("AssignerAccountRequests_Open")}</h3>
            <Table>
                <thead>
                    <tr>
                        <th>{resolveText("AssignerAccountRequest_ContactPersonName")}</th>
                        <th>{resolveText("AssignerAccountRequest_Email")}</th>
                        <th>{resolveText("AssignerAccountRequest_ExpectedAssignmentsPerYear")}</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {isLoadingRequests ? <LoadingTableRow colSpan={4} />
                    : requests.length === 0 ? <NoEntriesTableRow colSpan={4} />
                    : requests.map(request => (
                        <tr key={request.id}>
                            <td>
                                {request.contactPersonName}
                                <div className="text-secondary">
                                    <small>{request.note}</small>
                                </div>
                            </td>
                            <td>
                                <a href={`mailto:${request.email}`}>{request.email}</a>
                            </td>
                            <td>{request.expectedAssignmentsPerYear}</td>
                            <td>
                                <div className="d-flex gap-2 justify-content-end">
                                    <AsyncButton
                                        onClick={() => accept(request.id)}
                                        variant="success"
                                        isExecuting={isSubmitting}
                                        activeText={<><i className="fa fa-check" /> {resolveText("Accept")}</>}
                                        executingText={resolveText("Submitting...")}
                                    />
                                    <AsyncButton
                                        onClick={() => reject(request.id)}
                                        variant="danger"
                                        isExecuting={isSubmitting}
                                        activeText={<><i className="fa fa-times" /> {resolveText("Reject")}</>}
                                        executingText={resolveText("Submitting...")}
                                    />
                                </div>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </>
        : null}

        {isAssignersSectionVisible
        ? <>
            <h3>{resolveText("AssignerAccounts")}</h3>
            <Table>
                <thead>
                    <tr>
                        <th></th>
                        <th>{resolveText("AssignerAccount_Name")}</th>
                        <th>{resolveText("AssignerAccount_Username")}</th>
                        <th>{resolveText("AssignerAccount_Email")}</th>
                        <th>{resolveText("AssignerAccount_AssignedProfileCount")}</th>
                        <th>{resolveText("AssignerAccount_Status")}</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {isLoadingAssigners ? <LoadingTableRow colSpan={7} />
                    : assigners.length === 0 ? <NoEntriesTableRow colSpan={7} />
                    : assigners.map(assigner => (
                        <tr key={assigner.id}>
                            <td>
                                <Button variant="outline-secondary" size="sm" onClick={() => setSelectedAssigner(assigner)}>
                                    <i className="fa fa-info-circle" /> {resolveText("Details")}
                                </Button>
                            </td>
                            <td>{assigner.name}</td>
                            <td>{assigner.username}</td>
                            <td>{assigner.contact?.email}</td>
                            <td>{assigner.assignedProfileCount}</td>
                            <td>
                                <Badge bg={assigner.isActivated ? 'success' : 'secondary'}>
                                    {resolveText(assigner.isActivated ? "AssignerAccountStatus_Activated" : "AssignerAccountStatus_Deactivated")}
                                </Badge>
                            </td>
                            <td>
                                <div className="d-flex justify-content-end">
                                    {assigner.isActivated
                                    ? <AsyncButton
                                        variant="danger"
                                        isExecuting={submittingAssignerId === assigner.id}
                                        onClick={() => confirmDeactivation(assigner)}
                                        activeText={<><i className="fa fa-ban" /> {resolveText("AssignerAccount_Deactivate")}</>}
                                        executingText={resolveText("Submitting...")}
                                    />
                                    : <AsyncButton
                                        variant="success"
                                        isExecuting={submittingAssignerId === assigner.id}
                                        onClick={() => changeAssignerActivation(assigner, true)}
                                        activeText={<><i className="fa fa-check" /> {resolveText("AssignerAccount_Activate")}</>}
                                        executingText={resolveText("Submitting...")}
                                    />}
                                </div>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </>
        : null}

        {selectedAssigner
        ? <AssignerAccountDetailsModal
            show
            assigner={selectedAssigner}
            onClose={() => setSelectedAssigner(undefined)}
        />
        : null}
    </>);

};
export default AssignerAccountRequestsPage;
