import { useEffect, useState } from 'react';
import { Row, Col, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface HomePageProps {}

export const HomePage = (props: HomePageProps) => {

    const [ randomId, setRandomId ] = useState<string>();
    const navigate = useNavigate();

    useEffect(() => {
        const today = new Date();
        const getRandomId = async () => {
            try {
                const response = await apiClient.instance!.get(`api/id/random`, { birthdate: today.toISOString() });
                const result = await response.text();
                setRandomId(result);
            } catch {
                // Do nothing
            }
        };
        getRandomId();
    }, []);

    return (
        <>
            <h1 className='mt-3'>웃ID - Mensch.ID</h1>
            <p>
                Create your global ID.
            </p>
            {randomId
            ? <p>
                Sneak peak for humans born today: <b>{randomId}</b>
            </p>
            : null}
            <h2>{resolveText("HowToUseMenschID_Title")}</h2>
            <p>
                {resolveText("HowToUseMenschID_Text")}
            </p>
            <h2>{resolveText("SendChallenge")}</h2>
            <p>
                {resolveText("SendChallenge_Text")}
            </p>
            <Row>
                <Col/>
                <Col xs="auto">
                    <Button onClick={() => navigate("/challenge")}>{resolveText("SendChallenge")}</Button>
                </Col>
                <Col/>
            </Row>
        </>
    );

}