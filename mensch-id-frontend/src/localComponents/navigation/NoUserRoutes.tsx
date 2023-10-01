import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes";
import { CommonRoutes } from "./CommonRoutes";
import { Models } from "../types/models";
import RequestAssignerAccountPage from "../pages/Assigner/RequestAssignerAccountPage";
import HomePage from "../pages/HomePage";
import LoginPage from "../pages/UserManagement/LoginPage";
import LoginRedirectPage from "../pages/UserManagement/LoginRedirectPage";
import RequestPasswordResetPage from "../pages/UserManagement/RequestPasswordResetPage";
import ResetPasswordPage from "../pages/UserManagement/ResetPasswordPage";
import VerifyEmailPage from "../pages/UserManagement/VerifyEmailPage";

interface NoUserRoutesProps {
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}
export const NoUserRoutes = (props: NoUserRoutesProps): RouteDefinition[] => {

    const { onLoggedIn } = props;

    const routes: RouteDefinition[] = [
        { path: '/login', element: <LoginPage onLoggedIn={onLoggedIn} />},
        { path: '/login/redirect', element: <LoginRedirectPage onLoggedIn={onLoggedIn} />},
        { path: '/verify-email', element: <VerifyEmailPage /> },
        { path: '/reset-password', element: <ResetPasswordPage onLoggedIn={onLoggedIn} /> },
        { path: '/request-password-reset', element: <RequestPasswordResetPage /> },
        { path: '/request-assigner-account', element: <RequestAssignerAccountPage /> },
        ...CommonRoutes(),
        //{ path: '/pilot-project-hd', element: <PilotProjectHeidelbergPage /> },
        { path: '/', element: <HomePage isLoggedIn={false} /> },
        { path: '*', element: <LoginPage onLoggedIn={onLoggedIn} /> }
    ];

    return routes
}