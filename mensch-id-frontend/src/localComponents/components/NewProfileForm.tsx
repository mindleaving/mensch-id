import { FormEvent, useEffect, useState } from 'react';
import { Button, Col, Form, FormGroup, FormLabel, ListGroup, Row } from 'react-bootstrap';
import { StoreButton } from '../../sharedCommonComponents/components/StoreButton';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../types/models';
import { ViewModels } from '../types/viewModels';
import { BirthdayForm } from './BirthdayForm';

interface NewProfileFormProps {
    onProfileCreated: (profileData: Models.Person) => void;
}

export const NewProfileForm = (props: NewProfileFormProps) => {

    const [ birthDate, setBirthDate ] = useState<string>('');
    const [ newProfileData, setNewProfileData ] = useState<ViewModels.NewProfileViewModel>();
    const [ selectedId, setSelectedId ] = useState<string>();
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    useEffect(() => {
        if(!newProfileData) {
            return;
        }
        setSelectedId(newProfileData.idCandidates[0]);
    }, [ newProfileData ]);

    if(!newProfileData) {
        return (<BirthdayForm
            onNewProfileCreated={(birthDate,newProfileData) => {
                setBirthDate(birthDate);
                setNewProfileData(newProfileData);
            }}
        />);
    }
    
    const createProfile = async (e?: FormEvent) => {
        e?.preventDefault();
        if(!selectedId) {
            return;
        }
        setIsSubmitting(true);
        await sendPostRequest(
            `api/profiles/claim/${selectedId}`,
            resolveText('Profile_CouldNotStore'),
            null,
            async response => {
                const profileData = await response.json() as Models.Person;
                props.onProfileCreated(profileData);
            },
            undefined,
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
                                    readOnly
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