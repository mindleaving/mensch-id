import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes";
import { CommonRoutes } from "./CommonRoutes";
import { ViewModels } from "../types/viewModels";
import AssignerAccountManagementPage from "../pages/Assigner/AssignerAccountManagementPage";
import AssignerPage from "../pages/Assigner/AssignerPage";
import PrintCertificatePage from "../pages/Assigner/PrintCertificatePage";
import HomePage from "../pages/HomePage";
import NotFoundPage from "../pages/NotFoundPage";
import AssignerShopPage from "../pages/Shop/AssignerShopPage";
import MyOrdersPage from "../pages/Shop/MyOrdersPage";

interface AssignerRoutesProps {
    setUserViewModel: (profileData: ViewModels.AssignerAccountViewModel) => void;
}
export const AssignerRoutes = (props: AssignerRoutesProps) => {
    const { setUserViewModel } = props;

    const routes: RouteDefinition[] = [
        { path: '/account', element: <AssignerAccountManagementPage setUserViewModel={setUserViewModel} /> },
        { path: '/assigner', element: <AssignerPage setUserViewModel={setUserViewModel} /> },
        { path: '/print/certificate/:personId', element: <PrintCertificatePage /> },
        { path: '/shop', element: <AssignerShopPage /> },
        { path: '/shop/orders', element: <MyOrdersPage /> },

        ...CommonRoutes(),
        { path: '/', element: <HomePage isLoggedIn={true} /> },
        { path: '*', element: <NotFoundPage /> }
    ];

    return routes;
}