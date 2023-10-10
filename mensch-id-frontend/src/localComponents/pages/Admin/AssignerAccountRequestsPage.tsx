import { useState, useEffect } from "react";
import { Table } from "react-bootstrap";
import { LoadingTableRow } from "../../../sharedCommonComponents/components/LoadingTableRow";
import { NoEntriesTableRow } from "../../../sharedCommonComponents/components/NoEntriesTableRow";
import { openConfirmDeleteAlert, showSuccessAlert } from "../../../sharedCommonComponents/helpers/AlertHelpers";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { buildLoadObjectFunc } from "../../../sharedCommonComponents/helpers/LoadingHelpers";
import { sendPostRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { Models } from "../../types/models";
import { deleteObject } from "../../../sharedCommonComponents/helpers/DeleteHelpers";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";

interface AssignerAccountRequestsPageProps {}

export const AssignerAccountRequestsPage = (props: AssignerAccountRequestsPageProps) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ requests, setRequests ] = useState<Models.AssignerAccountRequest[]>([]);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    useEffect(() => {
        setIsLoading(true);
        const loadRequests = buildLoadObjectFunc(
            'api/admin/assigner-requests', {},
            resolveText("AssignerAccountRequests_CouldNotLoad"),
            setRequests,
            undefined,
            () => setIsLoading(false)
        );
        loadRequests();
    }, []);

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
            resolveText("AssignerAccountRequest_SuccessfullyDeleted"),
            resolveText("AssignerAccountRequest_CouldNotDelete"),
            () => {
                setRequests(state => state.filter(x => x.id !== id));
            },
            undefined,
            () => setIsSubmitting(false)
        )
    }

    const accept = async (id: string) => {
        setIsSubmitting(true);
        await sendPostRequest(
            `api/admin/assigner-requests/${id}/approve`, {},
            resolveText("AssignerAccountRequest_CouldNotApprove"),
            null,
            () => {
                showSuccessAlert(resolveText("AssignerAccountRequest_SuccessfullyApproved"))
                setRequests(state => state.filter(x => x.id !== id));
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    return (
    <>
        <h1>{resolveText("AssignerAccountRequests")}</h1>
        <Table>
            <thead>
                <tr>
                    <th></th>
                    <th>{resolveText("AssignerAccountRequest_ContactPersonName")}</th>
                    <th>{resolveText("AssignerAccountRequest_Email")}</th>
                    <th>{resolveText("AssignerAccountRequest_ExpectedAssignmentsPerYear")}</th>
                    <th></th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                {isLoading ? <LoadingTableRow colSpan={6} />
                : requests.length === 0 ? <NoEntriesTableRow colSpan={6} />
                : requests.map(request => (
                    <tr key={request.id}>
                        <td>
                            <i 
                                className="fa fa-trash red clickable"
                                onClick={() => {
                                    if(!isSubmitting) {
                                        reject(request.id);
                                    }
                                }}
                            />
                        </td>
                        <td>
                            {request.contactPersonName}
                            <div className="text-secondary">
                                <small>{request.note}</small>
                            </div>
                        </td>
                        <td>
                            <a href={`mailto:${request.email}`}>{request.email}</a>
                        </td>
                        <td>
                            {request.expectedAssignmentsPerYear}
                        </td>
                        <td>
                            <AsyncButton
                                onClick={() => accept(request.id)}
                                variant="success"
                                isExecuting={isSubmitting}
                                activeText={<><i className="fa fa-check" /> {resolveText("Accept")}</>}
                                executingText={resolveText("Submitting...")}
                            />
                        </td>
                        <td>
                            <AsyncButton
                                onClick={() => reject(request.id)}
                                variant="danger"
                                isExecuting={isSubmitting}
                                activeText={<><i className="fa fa-times" /> {resolveText("Reject")}</>}
                                executingText={resolveText("Submitting...")}
                            />
                        </td>
                    </tr>
                ))}
            </tbody>
        </Table>
    </>);

}
export default AssignerAccountRequestsPage;