import '../styles/profile.css';
import { useEffect, useState } from 'react';
import { Badge, Button, Col, Row } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { NotificationManager } from 'react-notifications';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { ApiError } from '../../sharedCommonComponents/communication/ApiError';
import { NewProfileForm } from '../components/NewProfileForm';
import { CopyButton } from '../../sharedCommonComponents/components/CopyButton';
import { LoginProvider } from '../types/enums.d';
import { useContext } from 'react';
import UserContext from '../contexts/UserContext';

interface ProfilePageProps {}

export const ProfilePage = (props: ProfilePageProps) => {

    const { profileData, setProfileData } = useContext(UserContext);
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
    }, [ isNewProfile ]);


    if(isLoading) {
        return (<h3>{resolveText("Loading...")}</h3>);
    }


    if(isNewProfile) {
        return (<>
            <h1>{resolveText("NewProfile")}</h1>
            <NewProfileForm
                onProfileCreated={(newProfile) => {
                    setProfileData({
                        ...newProfile,
                        loginProviders: [],
                        verifications: []
                    });
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
            <div className='d-flex align-items-center'>
                <div><h1>{resolveText("MyProfile")}</h1></div>
                {profileData.verifications.length > 0 
                ? <Badge 
                    pill 
                    bg="light"
                    text="success"
                    className='ms-3'
                >
                    <i className='fa fa-check green'/> {resolveText("Verified")} ({profileData.verifications.length})
                </Badge> 
                : null}
            </div>
            <Row className="mt-3">
                <Col>
                    <div className='d-flex align-items-center'>
                        <div className='me-3'>
                            {resolveText("Accounts")}: 
                        </div>
                        {profileData.loginProviders.map((loginProvider,loginIndex) => {
                            const iconLookup: { [key: string]: string } = {
                                [LoginProvider.Google]: 'google',
                                [LoginProvider.Twitter]: 'twitter',
                                [LoginProvider.Facebook]: 'facebook',
                                [LoginProvider.Microsoft]: 'windows'
                            };
                            const iconId = iconLookup[loginProvider] ?? 'user';
                            return (<Button
                                    key={loginIndex}
                                    variant="outline-success"
                                    disabled
                                    className='mx-2 login-provider-symbol'
                                    title={loginProvider}
                                >
                                    <i className={`fa fa-${iconId}`} />
                                </Button>
                            );
                        })}
                    </div>
                </Col>
            </Row>
            <Row className="mt-2">
                <Col xs={3}>{resolveText("Person_ID")}</Col>
                <Col xs={4} lg={2}>
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
                <Col xs={4} lg={2}>
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