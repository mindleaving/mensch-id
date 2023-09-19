import '../styles/profile.css';
import { useEffect, useState } from 'react';
import { Alert, Badge, Button, Col, Row } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { ApiError } from '../../sharedCommonComponents/communication/ApiError';
import { CopyButton } from '../../sharedCommonComponents/components/CopyButton';
import { LoginProvider } from '../types/enums.d';
import { useContext } from 'react';
import UserContext from '../contexts/UserContext';
import { useNavigate } from 'react-router-dom';
import { showErrorAlert } from '../../sharedCommonComponents/helpers/AlertHelpers';

interface ProfilePageProps {}

export const ProfilePage = (props: ProfilePageProps) => {

    const { profileData, setProfileData } = useContext(UserContext);
    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const navigate = useNavigate();

    useEffect(() => {
        setIsLoading(true);
        const loadProfileData = async () => {
            try {
                const response = await apiClient.instance!.get(`api/profiles/me`, {}, { handleError: false });
                if(!response.ok) {
                    if(response.status === 404) {
                        navigate("/new-profile");
                        return;
                    } else {
                        const errorText = await response.text();
                        throw new ApiError(response.status, errorText);
                    }
                } else {
                    const result = await response.json();
                    setProfileData(result);
                }
            } catch(error: any) {
                showErrorAlert(error.message, resolveText("Person_CouldNotLoad"));
            } finally {
                setIsLoading(false);
            }
        };
        loadProfileData();
    }, []);


    if(isLoading) {
        return (<h3>{resolveText("Loading...")}</h3>);
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
            <Row className='mt-3'>
                <Col>
                    <Alert
                        variant="success"
                    >
                        <Row className='align-items-center'>
                            <Col xs={3}>{resolveText("Person_ID")}</Col>
                            <Col xs={4} lg={2} className="px-2">
                                <strong className='text-nowrap'>{profileData.id}</strong>
                            </Col>
                            <Col>
                                <CopyButton
                                    value={profileData.id}
                                    size='sm'
                                />
                            </Col>
                        </Row>
                    </Alert>
                </Col>
            </Row>
            <hr />
            <div className='text-center'>
                <Button size="lg" onClick={() => navigate("/challenges")}>{resolveText("MyChallenges")}</Button>
            </div>
        </>
    );
}