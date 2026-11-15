import React, { useEffect, useState } from 'react';
import { Badge, Button, Table } from 'react-bootstrap';
import { openConfirmAlert, showSuccessAlert } from '../../sharedCommonComponents/helpers/AlertHelpers';
import { deleteObject } from '../../sharedCommonComponents/helpers/DeleteHelpers';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { buildLoadObjectFunc } from '../../sharedCommonComponents/helpers/LoadingHelpers';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { formatDateTime } from '../helpers/Formatter';
import { describeUserAgent, getSessionIconClassName, getSessionRevokePath } from '../helpers/SessionHelpers';
import { SessionType } from '../types/enums';
import { ViewModels } from '../types/viewModels';

interface ActiveSessionsProps {}

export const ActiveSessions = (props: ActiveSessionsProps) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ sessions, setSessions ] = useState<ViewModels.SessionViewModel[]>([]);
    const [ revokingSessionId, setRevokingSessionId ] = useState<string>();
    const [ isRevokingAll, setIsRevokingAll ] = useState<boolean>(false);

    useEffect(() => {
        const loadSessions = buildLoadObjectFunc<ViewModels.SessionViewModel[]>(
            'api/sessions', {},
            resolveText("Sessions_CouldNotLoad"),
            setSessions,
            undefined,
            () => setIsLoading(false)
        );
        loadSessions();
    }, []);

    const revokeSession = async (session: ViewModels.SessionViewModel) => {
        setRevokingSessionId(session.id);
        await deleteObject(
            getSessionRevokePath(session), {},
            resolveText("Session_CouldNotRevoke"),
            () => {
                showSuccessAlert(resolveText("Session_Revoked"));
                setSessions(state => state.filter(x => x.id !== session.id));
            },
            undefined,
            () => setRevokingSessionId(undefined)
        );
    };

    const revokeAllSessions = async () => {
        setIsRevokingAll(true);
        await sendPostRequest(
            'api/sessions/revoke-all', {},
            resolveText("Sessions_CouldNotRevokeAll"),
            null,
            async () => {
                window.location.assign('/');
            },
            undefined,
            () => setIsRevokingAll(false)
        );
    };

    const confirmRevokeAllSessions = () => {
        openConfirmAlert({
            title: resolveText("Sessions_RevokeAll_ConfirmTitle"),
            message: resolveText("Sessions_RevokeAll_ConfirmMessage"),
            closeOnClickOutside: true,
            buttons: [
                { label: resolveText("Sessions_RevokeAll_Cancel") },
                { label: resolveText("Sessions_RevokeAll_Confirm"), onClick: revokeAllSessions }
            ]
        });
    };

    const formatSessionName = (session: ViewModels.SessionViewModel) => {
        const device = describeUserAgent(session.userAgent);
        if(session.type === SessionType.App) {
            const clientName = session.clientName ?? resolveText("Session_UnknownApp");
            return device ? `${clientName} – ${device}` : clientName;
        }
        return device ?? resolveText("Session_UnknownBrowser");
    };

    const formatTimestamp = (timestamp?: string | null) => {
        return timestamp ? formatDateTime(new Date(timestamp)) : '';
    };

    if(isLoading) {
        return (<h3>{resolveText("Loading...")}</h3>);
    }

    return (
        <>
            <h3>{resolveText("Sessions")}</h3>
            <Table>
                <thead>
                    <tr>
                        <th></th>
                        <th>{resolveText("Session_Device")}</th>
                        <th>{resolveText("Session_Created")}</th>
                        <th>{resolveText("Session_LastActivity")}</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {sessions.length > 0
                    ? sessions.map(session => (
                        <tr key={`${session.type}-${session.id}`}>
                            <td>
                                <i className={getSessionIconClassName(session)} title={resolveText(`SessionType_${session.type}`)} />
                            </td>
                            <td>
                                {formatSessionName(session)}
                                {session.isCurrent
                                ? <Badge pill bg="light" text="success" className="ms-2">{resolveText("Session_ThisDevice")}</Badge>
                                : null}
                            </td>
                            <td>{formatTimestamp(session.createdTimestamp)}</td>
                            <td>{formatTimestamp(session.lastActivityTimestamp)}</td>
                            <td>
                                {!session.isCurrent
                                ? <Button
                                    variant="outline-danger"
                                    size="sm"
                                    disabled={revokingSessionId === session.id}
                                    onClick={() => revokeSession(session)}
                                >
                                    {resolveText("Session_Revoke")}
                                </Button>
                                : null}
                            </td>
                        </tr>
                    ))
                    : <tr>
                        <td colSpan={5} className="text-center">{resolveText("NoEntries")}</td>
                    </tr>}
                </tbody>
            </Table>
            <Button
                variant="danger"
                disabled={isRevokingAll}
                onClick={confirmRevokeAllSessions}
            >
                {resolveText("Sessions_RevokeAll")}
            </Button>
        </>
    );
};
