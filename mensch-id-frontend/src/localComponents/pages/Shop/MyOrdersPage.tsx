import { useState, useEffect } from "react";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { buildLoadObjectFunc } from "../../../sharedCommonComponents/helpers/LoadingHelpers";
import { OrdersList } from "../../components/Shop/OrdersList";
import { OrderDirection } from "../../types/enums";
import { Models } from "../../types/models";

interface MyOrdersPageProps {}

export const MyOrdersPage = (props: MyOrdersPageProps) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ orders, setOrders ] = useState<Models.Shop.Order[]>([]);

    useEffect(() => {
        setIsLoading(true);
        const loadOrders = buildLoadObjectFunc(
            'api/shop/my-orders', { 'orderBy': 'time', orderDirection: OrderDirection.Descending },
            resolveText("Orders_CouldNotLoad"),
            setOrders,
            undefined,
            () => setIsLoading(false)
        );
        loadOrders();
    }, []);

    return (
    <>
        <h1>{resolveText("Orders")}</h1>
        <OrdersList
            isLoading={isLoading}
            orders={orders}
            onChange={updatedOrder => setOrders(state => {
                if(state.some(x => x.id === updatedOrder.id)) {
                    return state.map(order => order.id === updatedOrder.id ? updatedOrder : order);
                }
                return state.concat(updatedOrder);
            })}
        />
    </>);

}
export default MyOrdersPage;