import { useEffect, useState } from 'react';
import { Col, Row } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { Models } from '../types/models';
import { NotificationManager } from 'react-notifications';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { ApiError } from '../../sharedCommonComponents/communication/ApiError';
import { NewProfileForm } from '../components/NewProfileForm';
import { CopyButton } from '../../sharedCommonComponents/components/CopyButton';

interface ProfilePageProps {}

export const ProfilePage = (props: ProfilePageProps) => {

    const [ profileData, setProfileData ] = useState<Models.Person>();
    const [ isNewProfile, setIsNewProfile ] = useState<boolean>(false);
    const [ isLoading, setIsLoading ] = useState<boolean>(true);

    useEffect(() => {
        setIsLoading(true);
        const loadProfileData = async () => {
            try {
                const response = await apiClient.instance!.get(`api/profiles/me`, {}, { handleError: false });
                if(!response.ok) {
                    if(response.status === 404) {
                        setIsNewProfile(true);
                    } else {
                        const errorText = await response.text();
                        throw new ApiError(response.status, errorText);
                    }
                } else {
                    const result = await response.json();
                    setProfileData(result);
                }
            } catch(error: any) {
                NotificationManager.error(error.message, resolveText("Person_CouldNotLoad"));
            } finally {
                setIsLoading(false);
            }
        };
        loadProfileData();
    }, []);


    if(isLoading) {
        return (<h3>{resolveText("Loading...")}</h3>);
    }


    if(isNewProfile) {
        return (<>
            <h1>{resolveText("NewProfile")}</h1>
            <NewProfileForm
                onProfileCreated={(newProfile) => {
                    setProfileData(newProfile);
                    setIsNewProfile(false);
                }}
            />
        </>);
    }

    if(!profileData) {
        return (<h3>{resolveText("Person_CouldNotLoad")}</h3>);
    }

    return (
        <>
            <h1>{resolveText("MyProfile")}</h1>
            <Row className="mt-3">
                <Col xs={3}>{resolveText("Person_ID")}</Col>
                <Col xs={2}>
                    <strong>{profileData.id}</strong>
                </Col>
                <Col>
                    <CopyButton
                        value={profileData.id}
                    />
                </Col>
            </Row>
            <Row className="mt-2">
                <Col xs={3}>{resolveText("Person_AnonymousID")}</Col>
                <Col xs={2}>
                    <strong>{profileData.anonymousId}</strong>
                </Col>
                <Col>
                    <CopyButton
                        value={profileData.anonymousId}
                    />
                </Col>
            </Row>
        </>
    );
}