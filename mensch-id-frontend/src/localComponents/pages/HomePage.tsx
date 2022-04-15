import React, { useEffect, useState } from 'react';
import { Button } from 'react-bootstrap';
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
                Create your global ID ... available in May 2022.
            </p>
            {randomId
            ? <p>
                Sneak peak for humans born today: <b>{randomId}</b>
            </p>
            : null}
            <Button onClick={() => navigate("/login")}>{resolveText("Login")}</Button>
        </>
    );

}