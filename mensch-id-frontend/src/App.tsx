import { apiClient, ApiClient } from './sharedCommonComponents/communication/ApiClient';
import { defaultGlobalizer, Globalizer } from './sharedCommonComponents/helpers/Globalizer';
import germanTranslation from './localComponents/resources/translation.de.json';
import englishTranslation from './localComponents/resources/translation.en.json';
import { Layout } from './localComponents/components/Layout';
import { useNavigate } from 'react-router-dom';
import { Models } from './localComponents/types/models';
import { useEffect, useMemo, useState } from 'react';
import { SessionStoreKeys } from './localComponents/types/frontendTypes.d';
import UserContext from './localComponents/contexts/UserContext';
import { ViewModels } from './localComponents/types/viewModels';
import { AccountType } from './localComponents/types/enums.d';
import { RoutesBuilder } from './sharedCommonComponents/navigation/RoutesBuilder';
import { NormalUserRoutes } from './localComponents/navigation/NormalUserRoutes';
import { AssignerRoutes } from './localComponents/navigation/AssignerRoutes';
import { NoUserRoutes } from './localComponents/navigation/NoUserRoutes';
import { PageContainer } from './localComponents/components/PageContainer';
import { AdminRoutes } from './localComponents/navigation/AdminRoutes';

defaultGlobalizer.instance = new Globalizer(
    navigator.languages.map(x => x.substring(0, 2)), 
    "en", 
    [germanTranslation, englishTranslation]);
apiClient.instance = window.location.hostname.toLowerCase() === "localhost"
    ? new ApiClient(window.location.hostname, 44321)
    : new ApiClient(window.location.hostname, 443);

function App() {
    const navigate = useNavigate();
    const [ isLoggedIn, setIsLoggedIn ] = useState<boolean>(false);
    const [ userViewModel, setUserViewModel ] = useState<ViewModels.IUserViewModel | undefined>(
        !!sessionStorage.getItem(SessionStoreKeys.UserViewModel) 
            ? JSON.parse(sessionStorage.getItem(SessionStoreKeys.UserViewModel)!)
            : undefined
    );
    const [ accountType, setAccountType ] = useState<AccountType>();

    useEffect(() => {
        if(!userViewModel) {
            return;
        }
        sessionStorage.setItem(SessionStoreKeys.UserViewModel, JSON.stringify(userViewModel));
    }, [ userViewModel ]);

    useEffect(() => {
        const checkLoginState = async () => {
            try {
                const response = await apiClient.instance!.get('api/accounts/is-logged-in', {}, { handleError: false });
                if(response.status === 200) {
                    const isAlreadyLoggedIn = isLoggedIn;
                    const loggedInInfo = await response.json() as Models.IsLoggedInResponse;
                    setIsLoggedIn(true);
                    setAccountType(loggedInInfo.accountType);
                    if(!isAlreadyLoggedIn && window.location.pathname === '/') {
                        navigateToAccountHomePage(loggedInInfo.accountType);
                    }
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

    const onLoggedIn = (isLoggedInResponse: Models.IsLoggedInResponse) => {
        setIsLoggedIn(true);
        setAccountType(isLoggedInResponse.accountType!);
        navigateToAccountHomePage(isLoggedInResponse.accountType!);
    }

    const navigateToAccountHomePage = (accountType: AccountType) => {
        switch (accountType) {
            case AccountType.Admin:
                navigate('/assigner-account-requests');
                break;
            case AccountType.Assigner:
                navigate('/assigner');
                break;
            default:
                navigate('/me');
                break;
        }
    };

    const onLogOut = () => {
        setUserViewModel(undefined);
        setAccountType(undefined);
        sessionStorage.removeItem(SessionStoreKeys.UserViewModel);
        setIsLoggedIn(false);
        navigate("/");
    }

    const routes = useMemo(() => {
        switch(accountType) {
            case AccountType.External:
            case AccountType.Local:
            case AccountType.LocalAnonymous:
                return NormalUserRoutes({
                    onLoggedIn: onLoggedIn,
                    setUserViewModel: setUserViewModel,
                    onLogOut: onLogOut
                });
            case AccountType.Assigner:
                return AssignerRoutes({
                    setUserViewModel: setUserViewModel
                });
            case AccountType.Admin:
                return AdminRoutes({});
            default:
                return NoUserRoutes({
                    onLoggedIn: onLoggedIn
                });
        }
    }, [ accountType ]);

    return (
    <UserContext.Provider value={userViewModel}>
        <Layout isLoggedIn={isLoggedIn} accountType={accountType} onLogOut={onLogOut}> 
            <RoutesBuilder
                routeDefinitions={routes}
                containerBuilder={children => <PageContainer>
                    {children}
                </PageContainer>}
            />
        </Layout>
    </UserContext.Provider>
    );
}

export default App;
