import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes";
import AssignerAccountManagementPage from "../pages/AssignerAccountManagementPage";
import { AssignerPage } from "../pages/AssignerPage";
import AssignerShopPage from "../pages/AssignerShopPage";
import { HomePage } from "../pages/HomePage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { PrintCertificatePage } from "../pages/PrintCertificatePage";
import { ViewModels } from "../types/viewModels";
import { CommonRoutes } from "./CommonRoutes";

interface AssignerRoutesProps {
    setUserViewModel: (profileData: ViewModels.AssignerAccountViewModel) => void;
}
export const AssignerRoutes = (props: AssignerRoutesProps) => {
    const { setUserViewModel } = props;

    const routes: RouteDefinition[] = [
        { path: '/account', element: <AssignerAccountManagementPage /> },
        { path: '/assigner', element: <AssignerPage setUserViewModel={setUserViewModel} /> },
        { path: '/print/certificate/:personId', element: <PrintCertificatePage /> },
        { path: '/shop', element: <AssignerShopPage /> },

        ...CommonRoutes(),
        { path: '/', element: <HomePage isLoggedIn={true} /> },
        { path: '*', element: <NotFoundPage /> }
    ];

    return routes;
}