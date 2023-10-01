import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes";
import AssignerAccountRequestsPage from "../pages/AssignerAccountRequestsPage";
import { HomePage } from "../pages/HomePage";
import { NotFoundPage } from "../pages/NotFoundPage";
import ShopOrdersPage from "../pages/ShopOrdersPage";
import { CommonRoutes } from "./CommonRoutes";

interface AdminRoutesProps {}

export const AdminRoutes = (props: AdminRoutesProps) => {

    const routes: RouteDefinition[] = [
        { path: '/assigner-account-requests', element: <AssignerAccountRequestsPage /> },
        { path: '/shop-orders', element: <ShopOrdersPage /> },
        ...CommonRoutes(),
        { path: '/', element: <HomePage isLoggedIn={true} /> },
        { path: '*', element: <NotFoundPage /> }
    ];

    return routes;

}