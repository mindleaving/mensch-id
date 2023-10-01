import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes";
import { CommonRoutes } from "./CommonRoutes";
import { Models } from "../types/models";
import { ViewModels } from "../types/viewModels";
import HomePage from "../pages/HomePage";
import MyChallengesPage from "../pages/MyChallengesPage";
import NewProfilePage from "../pages/NewProfilePage";
import NotFoundPage from "../pages/NotFoundPage";
import ProfilePage from "../pages/ProfilePage";
import AccountManagementPage from "../pages/UserManagement/AccountManagementPage";
import LinkAccountPage from "../pages/UserManagement/LinkAccountPage";
import LinkAccountRedirectPage from "../pages/UserManagement/LinkAccountRedirectPage";
import LoginRedirectPage from "../pages/UserManagement/LoginRedirectPage";

interface NormalUserRoutesProps {
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
    setUserViewModel: (profileData: ViewModels.ProfileViewModel) => void;
}
export const NormalUserRoutes = (props: NormalUserRoutesProps) => {
    const { onLoggedIn, setUserViewModel } = props;

    const routes: RouteDefinition[] = [
        { path: '/linkaccount', element: <LinkAccountPage onLoggedIn={onLoggedIn} /> },
        { path: '/linkaccount/finish', element: <LinkAccountRedirectPage /> },
        { path: '/login/redirect', element: <LoginRedirectPage onLoggedIn={onLoggedIn} />},
        { path: '/me', element: <ProfilePage setUserViewModel={setUserViewModel} /> },
        { path: '/new-profile', element: <NewProfilePage /> },
        { path: '/challenges', element: <MyChallengesPage /> },
        { path: '/accounts', element: <AccountManagementPage /> },
        ...CommonRoutes(),
        { path: '/', element: <HomePage isLoggedIn={true} /> },
        { path: '*', element: <NotFoundPage /> }
    ];

    return routes;
}