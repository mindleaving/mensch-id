import { useEffect, useState } from 'react';
import { Button, Image } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { Center } from '../../sharedCommonComponents/components/Center';
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
                {resolveText("Home_ClaimYouGlobalID")}
            </p>
            {randomId
            ? <p>
                {resolveText("Home_SneakPeak")}: <b>{randomId}</b>
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
            <Center>
                <Button onClick={() => navigate("/challenge")}>{resolveText("SendChallenge")}</Button>
            </Center>
            <Center className='mt-5'>
                <Image
                    src='/img/blood-sample-tube-mensch-id.jpg'
                    width={450}
                    alt="Blood sample tube with 웃ID on it"
                />
            </Center>
        </>
    );

}