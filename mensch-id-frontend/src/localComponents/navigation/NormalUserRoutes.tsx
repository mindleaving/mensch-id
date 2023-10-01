import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes";
import { AccountManagementPage } from "../pages/AccountManagementPage";
import { HomePage } from "../pages/HomePage";
import { LinkAccountPage } from "../pages/LinkAccountPage";
import { LinkAccountRedirectPage } from "../pages/LinkAccountRedirectPage";
import { LoginRedirectPage } from "../pages/LoginRedirectPage";
import { MyChallengesPage } from "../pages/MyChallengesPage";
import { NewProfilePage } from "../pages/NewProfilePage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { ProfilePage } from "../pages/ProfilePage";
import RequestAssignerAccountPage from "../pages/RequestAssignerAccountPage";
import { RequestPasswordResetPage } from "../pages/RequestPasswordResetPage";
import { ResetPasswordPage } from "../pages/ResetPasswordPage";
import { VerifyEmailPage } from "../pages/VerifyEmailPage";
import { Models } from "../types/models";
import { ViewModels } from "../types/viewModels";
import { CommonRoutes } from "./CommonRoutes";

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

        { path: '/verify-email', element: <VerifyEmailPage /> },
        { path: '/reset-password', element: <ResetPasswordPage onLoggedIn={onLoggedIn} /> },
        { path: '/request-password-reset', element: <RequestPasswordResetPage /> },
        { path: '/request-assigner-account', element: <RequestAssignerAccountPage /> },
        ...CommonRoutes(),
        { path: '/', element: <HomePage isLoggedIn={true} /> },
        { path: '*', element: <NotFoundPage /> }
    ];

    return routes;
}