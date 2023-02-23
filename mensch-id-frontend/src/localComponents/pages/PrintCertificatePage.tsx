import { useContext, useEffect, useState } from 'react';
import { Alert, Button, Col, Container, Row } from 'react-bootstrap';
import { useParams } from 'react-router-dom';
import { Center } from '../../sharedCommonComponents/components/Center';
import { LoadingAlert } from '../../sharedCommonComponents/components/LoadingAlert';
import { VerbatimText } from '../../sharedCommonComponents/components/VerbatimText';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { buildLoadObjectFunc } from '../../sharedCommonComponents/helpers/LoadingHelpers';
import { MenschId } from '../components/MenschId';
import UserContext from '../contexts/UserContext';
import { formatOwnershipSecret } from '../helpers/OwnershipSecretFormatter';
import '../styles/print-certificate.css';
import { ViewModels } from '../types/viewModels';

interface PrintCertificatePageProps {}

export const PrintCertificatePage = (props: PrintCertificatePageProps) => {

    const { personId } = useParams();
    const [ isLoading, setIsLoading ] = useState<boolean>(false);
    const [ assignerControlledProfile, setAssignerControlledProfile ] = useState<ViewModels.SignedAssignerControlledProfile>();
    const { assignerProfile } = useContext(UserContext);

    useEffect(() => {
        if(!personId) {
            return;
        }
        setIsLoading(true);
        const loadProfile = buildLoadObjectFunc(
            `api/assigner/profiles/${personId}`, { signed: 'true' },
            resolveText("AssignerControlledProfile_CouldNotLoad"),
            setAssignerControlledProfile,
            undefined,
            () => setIsLoading(false)
        );
        loadProfile();
    }, []);

    const openPrintDialogue = () => {
        window.print();
    }

    if(isLoading) {
        return (<LoadingAlert />);
    }
    
    if(!assignerControlledProfile) {
        return (<Alert variant='danger'>
            {resolveText("AssignerControlledProfile_CouldNotLoad")}
        </Alert>);
    }

    return (
    <Container className=''>
        <Row className='no-print'>
            <Col />
            <Col xs="auto">
                <Button
                    onClick={openPrintDialogue}
                    className='mb-2'
                >
                    <i className='fa fa-print' /> {resolveText("Print")}
                </Button>
            </Col>
        </Row>
        <div id='logo'>
            <MenschId />
        </div>
        {assignerProfile
        ? <div id='assigner-logo'>
            {assignerProfile.logoUrl ? <img src={assignerProfile.logoUrl} width='100%' alt='assigner-logo' className='mb-2' /> 
            : assignerProfile.name ? <span id='assigner-name'>{assignerProfile.name}</span> 
            : null}
        </div> : null}
        <div id='certificate-info'>
            <p className='mt-5'>
                {resolveText("Certificate_Congratulations")}
            </p>
            <Center className='my-2'>
                <h3>{assignerControlledProfile.id}</h3>
            </Center>
            <p className='mt-3'>
                {resolveText("Certificate_TakeControl").replace("{menschIdUrl}", "https://mensch.id/login")}
            </p>
            <p>
                {resolveText("Certificate_YouOwnershipSecret")}
            </p>
            <Center>
                <h3>{formatOwnershipSecret(assignerControlledProfile.ownershipSecret)}</h3>
            </Center>
        </div>
        <div id='about-menschid'>
            <h5>{resolveText("Certificate_AboutTitle")}</h5>
            <VerbatimText 
                text={resolveText("Certificate_AboutText")}
            />
            <hr />
            <div id='signature'>
                <small>Time: {new Date(assignerControlledProfile.timestamp).toISOString()}, Signature: {assignerControlledProfile.signature}</small>
            </div>
        </div>
    </Container>
    );

}