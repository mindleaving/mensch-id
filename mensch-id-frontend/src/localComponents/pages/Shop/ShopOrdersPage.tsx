import { useState, useEffect } from "react";
import { Button, FormCheck, FormGroup, Table } from "react-bootstrap";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";
import { LoadingTableRow } from "../../../sharedCommonComponents/components/LoadingTableRow";
import { NoEntriesTableRow } from "../../../sharedCommonComponents/components/NoEntriesTableRow";
import { showSuccessAlert } from "../../../sharedCommonComponents/helpers/AlertHelpers";
import { deleteObject } from "../../../sharedCommonComponents/helpers/DeleteHelpers";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { buildLoadObjectFunc } from "../../../sharedCommonComponents/helpers/LoadingHelpers";
import { sendPostRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { OrderStatus } from "../../types/enums.d";
import { Models } from "../../types/models";
import { OrderViewerModal } from "../../modals/OrderViewerModal";
import { QueryParameters } from "../../../sharedCommonComponents/types/frontendTypes";
import { OrderDirection } from "../../../sharedCommonComponents/types/enums.d";

interface ShopOrdersPageProps {}

export const ShopOrdersPage = (props: ShopOrdersPageProps) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ onlyOpenOrders, setOnlyOpenOrders ] = useState<boolean>(true);
    const [ orders, setOrders ] = useState<Models.Shop.Order[]>([]);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ showOrderModal, setShowOrderModal ] = useState<boolean>(false);
    const [ selectedOrder, setSelectedOrder ] = useState<Models.Shop.Order>();

    useEffect(() => {
        setIsLoading(true);
        const queryParams: QueryParameters = [
            { key: 'orderBy', value: 'time' },
            { key: 'orderDirection', value: OrderDirection.Ascending }
        ];
        if(onlyOpenOrders) {
            queryParams.push({ key: 'status', value: OrderStatus.Placed });
            queryParams.push({ key: 'status', value: OrderStatus.Accepted });
        }
        const loadOrders = buildLoadObjectFunc(
            'api/shop/orders', queryParams,
            resolveText("Orders_CouldNotLoad"),
            setOrders,
            undefined,
            () => setIsLoading(false)
        );
        loadOrders();
    }, [ onlyOpenOrders ]);

    const accept = async (id: string) => {
        setIsSubmitting(true);
        await sendPostRequest(
            `api/shop/orders/${id}/accept`, {},
            resolveText("Order_CouldNotAccept"),
            null,
            async response => {
                const updatedOrder = await response.json() as Models.Shop.Order;
                setOrders(state => state.map(order => order.id === id ? updatedOrder : order));
                showSuccessAlert(resolveText("Order_SuccessfullyAccepted"));
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    const markAsFulfilled = async (id: string) => {
        setIsSubmitting(true);
        await sendPostRequest(
            `api/shop/orders/${id}/fulfill`, {},
            resolveText("Order_CouldNotFulfill"),
            null,
            async response => {
                const updatedOrder = await response.json() as Models.Shop.Order;
                setOrders(state => state.map(order => order.id === id ? updatedOrder : order));
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

    const openOrder = (order: Models.Shop.Order) => {
        setSelectedOrder(order);
        setShowOrderModal(true);
    }

    return (
    <>
        <h1>{resolveText("Orders")}</h1>
        <FormGroup className="my-2">
            <FormCheck
                label={resolveText("Orders_ShowOnlyOpen")}
                checked={onlyOpenOrders}
                onChange={e => setOnlyOpenOrders(e.target.checked)}
            />
        </FormGroup>
        <Table>
            <thead>
                <tr>
                    <th></th>
                    <th>{resolveText("Order_InvoiceAddress")}</th>
                    <th>{resolveText("Order_Items")}</th>
                    <th>{resolveText("Order_Status")}</th>
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
                            <span className={
                                order.status === OrderStatus.Cancelled ? 'text-secondary' 
                                : [ OrderStatus.Shipped, OrderStatus.Received ].includes(order.status) ? 'text-success'
                                : order.status === OrderStatus.Returned ? 'text-danger'
                                : 'text-dark'
                            }>
                                <strong>{resolveText(`OrderStatus_${order.status}`)}</strong>
                            </span>
                        </td>
                        <td>
                            {order.status === OrderStatus.Placed
                            ? <AsyncButton
                                onClick={() => accept(order.id)}
                                isExecuting={isSubmitting}
                                activeText={resolveText("Order_Accept")}
                                executingText={resolveText("Order_Accept")}
                                variant="success"
                                className="me-2"
                            /> : null}
                            {[ OrderStatus.Placed, OrderStatus.Accepted ].includes(order.status)
                            ? <AsyncButton
                                onClick={() => markAsFulfilled(order.id)}
                                isExecuting={isSubmitting}
                                activeText={resolveText("Order_MarkAsFulfilled")}
                                executingText={resolveText("Order_MarkAsFulfilled")}
                                variant="success"
                                className="me-2"
                            /> : null}
                        </td>
                        <td>
                            <Button
                                onClick={() => openOrder(order)}
                            >
                                {resolveText("Open")}
                            </Button>
                        </td>
                    </tr>
                ))}
            </tbody>
        </Table>
        {selectedOrder
        ? <OrderViewerModal
            show={showOrderModal}
            onClose={() => setShowOrderModal(false)}
            order={selectedOrder}
        /> : null}
    </>);

}
export default ShopOrdersPage;