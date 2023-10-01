import { useState, useEffect } from "react";
import { Table, Button } from "react-bootstrap";
import { LoadingTableRow } from "../../../sharedCommonComponents/components/LoadingTableRow";
import { NoEntriesTableRow } from "../../../sharedCommonComponents/components/NoEntriesTableRow";
import { showSuccessAlert } from "../../../sharedCommonComponents/helpers/AlertHelpers";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { buildLoadObjectFunc } from "../../../sharedCommonComponents/helpers/LoadingHelpers";
import { sendPostRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { Models } from "../../types/models";

interface AssignerAccountRequestsPageProps {}

export const AssignerAccountRequestsPage = (props: AssignerAccountRequestsPageProps) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ requests, setRequests ] = useState<Models.AssignerAccountRequest[]>([]);

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

    const reject = async (id: string) => {

    }

    const accept = async (id: string) => {
        await sendPostRequest(
            `api/admin/assigner-requests/${id}/approve`, {},
            resolveText("AssignerAccountRequest_CouldNotApprove"),
            null,
            () => {
                showSuccessAlert(resolveText("AssignerAccountRequest_SuccessfullyApproved"))
                setRequests(state => state.map(request => {
                    if(request.id === id) {
                        return {
                            ...request,
                            isApproved: true
                        };
                    }
                    return request;
                }))
            }
        )
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
                                onClick={() => reject(request.id)}
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
                            <Button
                                onClick={() => accept(request.id)}
                                variant="success"
                            >
                                <i className="fa fa-check" /> {resolveText("Accept")}
                            </Button>
                        </td>
                        <td>
                            <Button
                                onClick={() => reject(request.id)}
                                variant="danger"
                            >
                                <i className="fa fa-times" /> {resolveText("Reject")}
                            </Button>
                        </td>
                    </tr>
                ))}
            </tbody>
        </Table>
    </>);

}
export default AssignerAccountRequestsPage;