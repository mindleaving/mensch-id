import { useEffect, useState } from "react";
import { resolveText } from "../../sharedCommonComponents/helpers/Globalizer";
import { MenschIdCommunityEvent } from "../types/frontendTypes";
import { showErrorAlert } from "../../sharedCommonComponents/helpers/AlertHelpers";
import { NoEntriesAlert } from "../../sharedCommonComponents/components/NoEntriesAlert";
import { Card, Table } from "react-bootstrap";
import { formatDateTime } from "../helpers/Formatter";
import { LoadingAlert } from "../../sharedCommonComponents/components/LoadingAlert";

interface EventsPageProps {}

export const EventsPage = (props: EventsPageProps) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ events, setEvents ] = useState<MenschIdCommunityEvent[]>([]);

    useEffect(() => {
        const loadEvents = async () => {
            try {
                setIsLoading(true);
                const response = await fetch('/events.json', {
                    method: 'GET'
                });
                if(!response.ok) {
                    showErrorAlert(resolveText("Events_CouldNotLoad"));
                    return;
                }
                const result = await response.json() as MenschIdCommunityEvent[];
                setEvents(result);
            } catch {
                showErrorAlert(resolveText("Events_CouldNotLoad"));
            } finally {
                setIsLoading(false);
            }
        }
        loadEvents();
    }, []);

    return (
    <>
        <h1>{resolveText("Events")}</h1>
        {isLoading ? <LoadingAlert />
        : events.length === 0 ? <NoEntriesAlert />
        : events.map((e,idx) => (
            <Card 
                key={idx}
                className="mb-2"
            >
                <Card.Header>
                    <Card.Title>{formatDateTime(new Date(e.startTime))} - {e.name}</Card.Title>
                </Card.Header>
                <Card.Body>
                    <Table>
                        <tbody>
                            <tr>
                                <td>{resolveText("MenschIdCommunityEvent_Name")}</td>
                                <td>{e.name}</td>
                            </tr>
                            <tr>
                                <td>{resolveText("MenschIdCommunityEvent_StartTime")}</td>
                                <td>{formatDateTime(new Date(e.startTime))}</td>
                            </tr>
                            {e.url
                            ? <tr>
                                <td>{resolveText("MenschIdCommunityEvent_Url")}</td>
                                <td><a href={e.url} target="_blank" rel="noreferrer">{e.url}</a></td>
                            </tr> : null}
                        </tbody>
                    </Table>
                </Card.Body>
            </Card>
        ))}
    </>
    );

}
export default EventsPage;