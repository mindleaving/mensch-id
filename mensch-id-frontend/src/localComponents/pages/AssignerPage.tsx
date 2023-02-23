import { useContext, useEffect, useState } from 'react';
import { Alert } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { buildLoadObjectFunc } from '../../sharedCommonComponents/helpers/LoadingHelpers';
import { AssignedProfilesTable } from '../components/Assigner/AssignedProfilesTable';
import { IdCandidateAlert } from '../components/Assigner/IdCandidateAlert';
import { NewIdCandidateForm } from '../components/Assigner/NewIdCandidateForm';
import UserContext from '../contexts/UserContext';

interface AssignerPageProps {}

export const AssignerPage = (props: AssignerPageProps) => {

    const [ birthDate, setBirthDate ] = useState<string>('');
    const [ idCandidate, setIdCandidate ] = useState<string>();
    const [ latestAssignedId, setLatestAssignedId ] = useState<string>();
    const { assignerProfile, setAssignerProfile} = useContext(UserContext)!;

    useEffect(() => {
        if(assignerProfile || !setAssignerProfile) {
            return;
        }
        const loadAssignerProfile = buildLoadObjectFunc(
            'api/assigner/me', {},
            resolveText("Assigner_CouldNotLoadProfile"),
            setAssignerProfile
        );
        loadAssignerProfile();
    }, []);

    const onIdCandidateAccepted = (id: string) => {
        setIdCandidate(undefined);
        setLatestAssignedId(id);
    }
    return (
        <>
            <h1>{resolveText("Assigner")}</h1>
            <div className='lead'>
                {resolveText("Assigner_Instructions")}
            </div>
            <hr />
            <Alert
                variant="secondary"
            >
            {!idCandidate
            ? <NewIdCandidateForm 
                birthDate={birthDate}
                onBirthDateChanged={setBirthDate}
                onNewIdCandidate={setIdCandidate}
            />
            : <IdCandidateAlert
                idCandidate={idCandidate}
                onAccepted={onIdCandidateAccepted}
                onRejected={() => setIdCandidate(undefined)}
            />}
            </Alert>
            <hr className='my-3' />
            <h3>{resolveText("AssignedIDs")}</h3>
            <AssignedProfilesTable latestAssignedId={latestAssignedId} />
        </>
    );

}