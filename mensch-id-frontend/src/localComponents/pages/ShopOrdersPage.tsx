import { Table } from "react-bootstrap";
import { resolveText } from "../../sharedCommonComponents/helpers/Globalizer";
import { Models } from "../types/models";
import { useState, useEffect } from "react";
import { buildLoadObjectFunc } from "../../sharedCommonComponents/helpers/LoadingHelpers";
import { LoadingTableRow } from "../../sharedCommonComponents/components/LoadingTableRow";
import { NoEntriesTableRow } from "../../sharedCommonComponents/components/NoEntriesTableRow";
import { sendPostRequest } from "../../sharedCommonComponents/helpers/StoringHelpers";
import { showSuccessAlert } from "../../sharedCommonComponents/helpers/AlertHelpers";
import { AsyncButton } from "../../sharedCommonComponents/components/AsyncButton";
import { deleteObject } from "../../sharedCommonComponents/helpers/DeleteHelpers";
import { OrderStatus } from "../types/enums.d";

interface ShopOrdersPageProps {}

export const ShopOrdersPage = (props: ShopOrdersPageProps) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ orders, setOrders ] = useState<Models.Shop.Order[]>([]);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    useEffect(() => {
        setIsLoading(true);
        const loadOrders = buildLoadObjectFunc(
            'api/shop/orders', { status: OrderStatus.Placed },
            resolveText("Orders_CouldNotLoad"),
            setOrders,
            undefined,
            () => setIsLoading(false)
        );
        loadOrders();
    }, []);

    const markAsFulfilled = async (id: string) => {
        setIsSubmitting(true);
        await sendPostRequest(
            `api/shop/orders/${id}/fulfill`, {},
            resolveText("Order_CouldNotFulfill"),
            null,
            async response => {
                const fulfilledOrder = await response.json() as Models.Shop.Order;
                setOrders(state => state.map(order => order.id === id ? fulfilledOrder : order));
                showSuccessAlert(resolveText("Order_SuccessfullyFulfilled"));
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    const reject = async (id: string) => {
        setIsSubmitting(true);
        await deleteObject(
            `api/shop/orders/${id}`, {},
            resolveText("Order_SuccessfullyDeleted"),
            resolveText("Order_CouldNotDelete"),
            () => {
                setOrders(state => state.filter(x => x.id !== id));
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    return (
    <>
        <h1>{resolveText("Orders")}</h1>
        <Table>
            <thead>
                <tr>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                {isLoading ? <LoadingTableRow colSpan={4} />
                : orders.length === 0 ? <NoEntriesTableRow colSpan={4} />
                : orders.map(order => (
                    <tr key={order.id}>
                        <td>
                            <i
                                className="fa fa-trash red clickable"
                                onClick={() => reject(order.id)}
                            />
                        </td>
                        <td>
                            {order.invoiceAddress.name}, {order.invoiceAddress.address.postalCode} {order.invoiceAddress.address.city}, {order.invoiceAddress.address.country}
                        </td>
                        <td>
                            {order.items.map(x => x.product.name).join(", ")}
                        </td>
                        <td>
                            {order.status === OrderStatus.Placed
                            ? <AsyncButton
                                onClick={() => markAsFulfilled(order.id)}
                                isExecuting={isSubmitting}
                                activeText={resolveText("Order_MarkAsFulfilled")}
                                executingText={resolveText("Order_MarkAsFulfilled")}
                                variant="success"
                            /> 
                            : <span className={
                                order.status === OrderStatus.Cancelled ? 'text-secondary' 
                                : order.status === OrderStatus.Fulfilled ? 'text-success'
                                : 'text-dark'
                            }>
                                <strong>{resolveText(`OrderStatus_${order.status}`)}</strong>
                            </span>}
                        </td>
                    </tr>
                ))}
            </tbody>
        </Table>
    </>);

}
export default ShopOrdersPage;