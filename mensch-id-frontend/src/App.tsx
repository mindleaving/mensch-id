import React, { useEffect, useState } from 'react';
import { apiClient, ApiClient } from './sharedCommonComponents/communication/ApiClient';
import { defaultGlobalizer, Globalizer, resolveText } from './sharedCommonComponents/helpers/Globalizer';
import germanTranslation from './localComponents/resources/translation.de.json';
import englishTranslation from './localComponents/resources/translation.en.json';
import { buildLoadObjectFunc } from './sharedCommonComponents/helpers/LoadingHelpers';
import { Layout } from './localComponents/components/Layout';

defaultGlobalizer.instance = new Globalizer("de", "en", [germanTranslation, englishTranslation]);
apiClient.instance = window.location.hostname.toLowerCase() === "localhost"
    ? new ApiClient(window.location.hostname, 44321)
    : new ApiClient(window.location.hostname, 443);

function App() {

    const [ randomId, setRandomId ] = useState<string>();
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
    <Layout>
        <h1 className='mt-3'>웃ID - Mensch.ID</h1>
        <p>
            Create your global ID ... available in May 2022.
        </p>
        {randomId
        ? <p>
            Sneak peak for humans born today: <b>{randomId}</b>
        </p>
        : null}
    </Layout>
    );
}

export default App;
