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
import { SendChallengePage } from './localComponents/pages/SendChallengePage';
import { MyChallengesPage } from './localComponents/pages/MyChallengesPage';
import { SessionStoreKeys } from './localComponents/types/frontendTypes.d';
import { ResetPasswordPage } from './localComponents/pages/ResetPasswordPage';
import { RequestPasswordResetPage } from './localComponents/pages/RequestPasswordResetPage';
import { VerifyEmailPage } from './localComponents/pages/VerifyEmailPage';
import UserContext from './localComponents/contexts/UserContext';
import { ViewModels } from './localComponents/types/viewModels';
import { AssignerPage } from './localComponents/pages/AssignerPage';
import { NewProfilePage } from './localComponents/pages/NewProfilePage';
import { AccountType } from './localComponents/types/enums.d';
import { AboutPage } from './localComponents/pages/AboutPage';
import { PilotProjectHeidelbergPage } from './localComponents/pages/PilotProjectHeidelbergPage';

defaultGlobalizer.instance = new Globalizer(
    navigator.languages.map(x => x.substring(0, 2)), 
    "en", 
    [germanTranslation, englishTranslation]);
apiClient.instance = window.location.hostname.toLowerCase() === "localhost"
    ? new ApiClient(window.location.hostname, 44321)
    : new ApiClient(window.location.hostname, 443);
apiClient.instance!.defaultOptions.includeCredentials = true;
if(!!sessionStorage.getItem(SessionStoreKeys.AccessToken)) {
    apiClient.instance!.setAccessToken(sessionStorage.getItem(SessionStoreKeys.AccessToken)!);
}

function App() {
    const navigate = useNavigate();
    const [ isLoggedIn, setIsLoggedIn ] = useState<boolean>(false);
    const [ profileData, setProfileData ] = useState<ViewModels.ProfileViewModel | undefined>();
    const [ accountType, setAccountType ] = useState<AccountType>();

    useEffect(() => {
        const checkLoginState = async () => {
            try {
                const response = await apiClient.instance!.get('api/accounts/is-logged-in', {});
                if(response.status === 200) {
                    const loggedInInfo = await response.json() as Models.IsLoggedInResponse;
                    setIsLoggedIn(true);
                    setAccountType(loggedInInfo.accountType);
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

    const onLoggedIn = (authenticationResult: Models.AuthenticationResult) => {
        if(!authenticationResult.isAuthenticated) {
            navigate("/login");
            return;
        }
        if(authenticationResult.accessToken) {
            apiClient.instance!.setAccessToken(authenticationResult.accessToken);
            sessionStorage.setItem(SessionStoreKeys.AccessToken, authenticationResult.accessToken);
        }
        setIsLoggedIn(true);
        setAccountType(authenticationResult.accountType!);
        if(authenticationResult.accountType === AccountType.Assigner) {
            navigate("/assigner");
        } else {
            navigate("/me");
        }
    }
    
    const logOut = async () => {
        await sendPostRequest(
            `api/accounts/logout`,
            resolveText('CouldNotLogOut'),
            null,
            () => {
                setProfileData(undefined);
                apiClient.instance!.setAccessToken('');
                sessionStorage.removeItem(SessionStoreKeys.AccessToken);
                setIsLoggedIn(false);
                navigate("/");
            }
        );
    }

    return (
    <UserContext.Provider value={{ profileData, setProfileData }}>
        <Layout isLoggedIn={isLoggedIn} accountType={accountType} onLogOut={logOut}>
            <Routes>
                {!isLoggedIn ? <Route path="/login" element={<LoginPage onLoggedIn={onLoggedIn} />} /> : null}
                <Route path="/login/redirect" element={<LoginRedirectPage onLoggedIn={onLoggedIn} />} />
                {isLoggedIn ? <Route path="/linkaccount" element={<LinkAccountPage onLoggedIn={onLoggedIn} />} /> : null}
                {isLoggedIn ? <Route path="/linkaccount/finish" element={<LinkAccountRedirectPage />} /> : null}
                {isLoggedIn && accountType !== AccountType.Assigner ? <Route path="/me" element={<ProfilePage />} /> : null}
                {isLoggedIn && accountType !== AccountType.Assigner ? <Route path="/new-profile" element={<NewProfilePage />} /> : null}
                {isLoggedIn && accountType !== AccountType.Assigner ? <Route path="/challenges" element={<MyChallengesPage />} /> : null}
                {isLoggedIn && accountType === AccountType.Assigner ? <Route path="/assigner" element={<AssignerPage />} /> : null}
                
                <Route path="/challenge" element={<SendChallengePage />} />
                <Route path='/verify-email' element={<VerifyEmailPage />} />
                <Route path='/reset-password' element={<ResetPasswordPage onLoggedIn={onLoggedIn} />} />
                <Route path='/request-password-reset' element={<RequestPasswordResetPage />} />
                <Route path="/privacy" element={<PrivacyPage />} />
                <Route path="/terms-of-service" element={<TermsOfServicePage />} />
                <Route path="/about" element={<AboutPage />} />
                <Route path="/pilot-project-hd" element={<PilotProjectHeidelbergPage />} />
                <Route path="/contact" element={<ContactPage />} />
                <Route path="/" element={<HomePage />} />
                <Route path="*" element={isLoggedIn ? <NotFoundPage /> : <LoginPage onLoggedIn={onLoggedIn} />} />
            </Routes>
        </Layout>
    </UserContext.Provider>
    );
}

export default App;
