import { RouteDefinition } from "../../sharedCommonComponents/types/frontendTypes";
import { CommonRoutes } from "./CommonRoutes";
import AssignerAccountRequestsPage from "../pages/Admin/AssignerAccountRequestsPage";
import HomePage from "../pages/HomePage";
import NotFoundPage from "../pages/NotFoundPage";
import ShopOrdersPage from "../pages/Shop/ShopOrdersPage";

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