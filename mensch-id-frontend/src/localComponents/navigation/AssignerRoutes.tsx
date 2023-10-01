import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes";
import AssignerAccountManagementPage from "../pages/AssignerAccountManagementPage";
import { AssignerPage } from "../pages/AssignerPage";
import AssignerShopPage from "../pages/AssignerShopPage";
import { HomePage } from "../pages/HomePage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { PrintCertificatePage } from "../pages/PrintCertificatePage";
import RequestAssignerAccountPage from "../pages/RequestAssignerAccountPage";
import { RequestPasswordResetPage } from "../pages/RequestPasswordResetPage";
import { ResetPasswordPage } from "../pages/ResetPasswordPage";
import { VerifyEmailPage } from "../pages/VerifyEmailPage";
import { Models } from "../types/models";
import { ViewModels } from "../types/viewModels";
import { CommonRoutes } from "./CommonRoutes";

interface AssignerRoutesProps {
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
    setUserViewModel: (profileData: ViewModels.AssignerAccountViewModel) => void;
}
export const AssignerRoutes = (props: AssignerRoutesProps) => {
    const { onLoggedIn, setUserViewModel } = props;

    const routes: RouteDefinition[] = [
        { path: '/account', element: <AssignerAccountManagementPage /> },
        { path: '/assigner', element: <AssignerPage setUserViewModel={setUserViewModel} /> },
        { path: '/print/certificate/:personId', element: <PrintCertificatePage /> },
        { path: '/shop', element: <AssignerShopPage /> },

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