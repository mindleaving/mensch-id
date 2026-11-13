import { Alert } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { TakeControlForm } from '../components/TakeControlForm';

interface NewProfilePageProps {}

export const NewProfilePage = (props: NewProfilePageProps) => {

    return (<>
        <h1>{resolveText("TakeOverControl")}</h1>
        <Alert variant="info">
            {resolveText("NewProfile_IdsAreIssuedByAssigners")}
        </Alert>
        <TakeControlForm />
    </>);

};
export default NewProfilePage;
