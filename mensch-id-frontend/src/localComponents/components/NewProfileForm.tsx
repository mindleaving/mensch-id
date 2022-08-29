import { FormEvent, useState } from 'react';
import { Button, Col, Form, FormControl, FormGroup, FormLabel, ListGroup, Row } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { StoreButton } from '../../sharedCommonComponents/components/StoreButton';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { loadObject } from '../../sharedCommonComponents/helpers/LoadingHelpers';
import { sendPutRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../types/models';
import { ViewModels } from '../types/viewModels';

interface NewProfileFormProps {
    onProfileCreated: (profileData: Models.Person) => void;
}

export const NewProfileForm = (props: NewProfileFormProps) => {

    const [ birthDate, setBirthDate ] = useState<string>('');
    const [ isLoading, setIsLoading ] = useState<boolean>(false);
    const [ newProfileData, setNewProfileData ] = useState<ViewModels.NewProfileViewModel>();

    const [ selectedId, setSelectedId ] = useState<string>();
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    const loadIdReservations = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsLoading(true);
        await loadObject<ViewModels.NewProfileViewModel>(
            `api/profiles/new`, { birthDate: birthDate },
            resolveText('NewProfile_CouldNotLoad'),
            vm => {
                setNewProfileData(vm);
                setSelectedId(vm.idCandidates[0]);
            },
            () => setIsLoading(false)
        );
    }

    if(!newProfileData) {
        return (
            <Form onSubmit={loadIdReservations}>
                <FormGroup>
                    <FormLabel>{resolveText("NewProfile_BirthDate")}</FormLabel>
                    <FormControl
                        value={birthDate}
                        onChange={(e: any) => setBirthDate(e.target.value)}
                        pattern='[0-9]{4}-[0-9]{2}-[0-9]{2}'
                        placeholder='Format: yyyy-MM-dd'
                    />
                </FormGroup>
                <AsyncButton
                    className='m-2'
                    type='submit'
                    activeText={resolveText("Submit")}
                    executingText={resolveText("Submitting...")}
                    isExecuting={isLoading}
                />
            </Form>
        )
    }
    
    const createProfile = async (e?: FormEvent) => {
        e?.preventDefault();
        if(!selectedId) {
            return;
        }
        const profileData: Models.Person = {
            id: selectedId
        };
        setIsSubmitting(true);
        await sendPutRequest(
            `api/profiles/${profileData.id}`,
            resolveText('Profile_CouldNotStore'),
            profileData,
            () => props.onProfileCreated(profileData),
            () => setIsSubmitting(false)
        );
    }

    return (
        <>
        <div className='d-flex align-items-center'>
            <div>
                <strong>{resolveText("BirthDate")}:</strong>
            </div>
            <div className='mx-2'>
                {birthDate}
            </div>
            <Button variant="link" onClick={() => setNewProfileData(undefined)}>
                {resolveText("NewProfile_ChangeBirthDate")}
            </Button>
        </div>
        <Form onSubmit={createProfile}>
            <FormGroup className='mt-2'>
                <FormLabel><h3>{resolveText("NewProfile_SelectMenschID")}</h3></FormLabel>
                <ListGroup>
                    {newProfileData.idCandidates.map(id => (
                        <ListGroup.Item
                            key={id}
                            onClick={() => setSelectedId(id)}
                            variant={selectedId === id ? 'success' : undefined}
                        >
                            <Form.Check>
                                <Form.Check.Input
                                    type="radio"
                                    checked={selectedId === id}
                                />
                                <Form.Check.Label>{id}</Form.Check.Label>
                            </Form.Check>
                        </ListGroup.Item>
                    ))}
                </ListGroup>
            </FormGroup>
            <Row>
                <Col>
                    <StoreButton
                        type="submit"
                        isStoring={isSubmitting}
                        disabled={!selectedId}
                    />
                </Col>
            </Row>
        </Form>
        </>
    );

}