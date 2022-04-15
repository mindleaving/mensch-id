import { apiClient, ApiClient } from './sharedCommonComponents/communication/ApiClient';
import { defaultGlobalizer, Globalizer, resolveText } from './sharedCommonComponents/helpers/Globalizer';
import germanTranslation from './localComponents/resources/translation.de.json';
import englishTranslation from './localComponents/resources/translation.en.json';
import { Layout } from './localComponents/components/Layout';
import { Route, Routes, useNavigate } from 'react-router-dom';
import { HomePage } from './localComponents/pages/HomePage';
import { NotFoundPage } from './localComponents/pages/NotFoundPage';
import { TermsOfServicePage } from './localComponents/pages/TermsOfServicePage';
import { PrivacyPage } from './localComponents/pages/PrivacyPage';
import { ProfilePage } from './localComponents/pages/ProfilePage';
import { LoginPage } from './localComponents/pages/LoginPage';
import { Models } from './localComponents/types/models';
import { sendPostRequest } from './sharedCommonComponents/helpers/StoringHelpers';
import { useEffect, useState } from 'react';
import { LoginRedirectPage } from './localComponents/pages/LoginRedirectPage';
import { LinkAccountPage } from './localComponents/pages/LinkAccountPage';
import { LinkAccountRedirectPage } from './localComponents/pages/LinkAccountRedirectPage';
import { ContactPage } from './localComponents/pages/ContactPage';

const accessTokenSessionStorageKey = "accesstoken";
defaultGlobalizer.instance = new Globalizer("de", "en", [germanTranslation, englishTranslation]);
apiClient.instance = window.location.hostname.toLowerCase() === "localhost"
    ? new ApiClient(window.location.hostname, 44321)
    : new ApiClient(window.location.hostname, 443);
if(!!sessionStorage.getItem(accessTokenSessionStorageKey)) {
    apiClient.instance!.setAccessToken(sessionStorage.getItem(accessTokenSessionStorageKey)!);
}
function App() {
    const navigate = useNavigate();
    const [ isLoggedIn, setIsLoggedIn ] = useState<boolean>(false);

    useEffect(() => {
        const checkLoginState = async () => {
            try {
                const response = await apiClient.instance!.get('api/accounts/is-logged-in', {});
                if(response.status === 200) {
                    setIsLoggedIn(true);
                } else if(response.status === 401) {
                    setIsLoggedIn(false);
                } else {
                    throw new Error(`Unexpected response code ${response.status}`);
                }
            } catch {
                setIsLoggedIn(false);
            }
        };
        checkLoginState();
    }, []);

    const onLoggedIn = (authenticationResult?: Models.AuthenticationResult) => {
        if(authenticationResult) {
            if(!authenticationResult.isAuthenticated) {
                navigate("/login");
                return;
            }
            apiClient.instance!.setAccessToken(authenticationResult.accessToken!);
            sessionStorage.setItem(accessTokenSessionStorageKey, authenticationResult.accessToken!);
        }
        setIsLoggedIn(true);
        navigate("/me");
    }
    const logOut = async () => {
        await sendPostRequest(
            `api/accounts/logout`,
            resolveText('CouldNotLogOut'),
            null,
            () => {
                apiClient.instance!.setAccessToken('');
                sessionStorage.removeItem('accesstoken');
                setIsLoggedIn(false);
                navigate("/");
            }
        );
    }

    return (
    <Layout isLoggedIn={isLoggedIn} onLogOut={logOut}>
        <Routes>
            {!isLoggedIn ? <Route path="/login" element={<LoginPage />} /> : null}
            <Route path="/login/redirect" element={<LoginRedirectPage onLoggedIn={onLoggedIn} />} />
            {isLoggedIn ? <Route path="/linkaccount" element={<LinkAccountPage />} /> : null}
            {isLoggedIn ? <Route path="/linkaccount/finish" element={<LinkAccountRedirectPage />} /> : null}
            {isLoggedIn ? <Route path="/me" element={<ProfilePage />} /> : null}
            <Route path="/privacy" element={<PrivacyPage />} />
            <Route path="/terms-of-service" element={<TermsOfServicePage />} />
            <Route path="/contact" element={<ContactPage />} />
            <Route path="/" element={<HomePage />} />
            <Route path="*" element={<NotFoundPage />} />
        </Routes>        
    </Layout>
    );
}

export default App;
