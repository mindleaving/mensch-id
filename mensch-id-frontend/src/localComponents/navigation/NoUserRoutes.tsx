import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes";
import { HomePage } from "../pages/HomePage";
import { LoginPage } from "../pages/LoginPage";
import { LoginRedirectPage } from "../pages/LoginRedirectPage";
import RequestAssignerAccountPage from "../pages/RequestAssignerAccountPage";
import { RequestPasswordResetPage } from "../pages/RequestPasswordResetPage";
import { ResetPasswordPage } from "../pages/ResetPasswordPage";
import { VerifyEmailPage } from "../pages/VerifyEmailPage";
import { Models } from "../types/models";
import { CommonRoutes } from "./CommonRoutes";

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