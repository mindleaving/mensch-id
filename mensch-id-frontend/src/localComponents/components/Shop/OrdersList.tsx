import { Button, Table } from "react-bootstrap";
import { Models } from "../../types/models";
import { LoadingTableRow } from "../../../sharedCommonComponents/components/LoadingTableRow";
import { NoEntriesTableRow } from "../../../sharedCommonComponents/components/NoEntriesTableRow";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { OrderViewerModal } from "../../modals/OrderViewerModal";
import { formatDateTime } from "../../helpers/Formatter";
import { useState } from "react";
import { OrderStatus } from "../../types/enums.d";
import { sendPostRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";

interface OrdersListProps {
    isLoading: boolean;
    orders: Models.Shop.Order[];
    onChange: (updatedOrder: Models.Shop.Order) => void;
}

export const OrdersList = (props: OrdersListProps) => {

    const { isLoading, orders, onChange } = props;
    const [ showOrderModal, setShowOrderModal ] = useState<boolean>(false);
    const [ selectedOrder, setSelectedOrder ] = useState<Models.Shop.Order>();
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    const openOrder = (order: Models.Shop.Order) => {
        setSelectedOrder(order);
        setShowOrderModal(true);
    }

    const cancel = async (id: string) => {
        setIsSubmitting(true);
        await sendPostRequest(
            `api/shop/orders/${id}/cancel`, {},
            resolveText("Order_CouldNotCancel"),
            null,
            async response => {
                const updatedOrder = await response.json() as Models.Shop.Order;
                onChange(updatedOrder);
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    const markAsReceived = async (id: string) => {
        setIsSubmitting(true);
        await sendPostRequest(
            `api/shop/orders/${id}/received`, {},
            resolveText("Order_CouldNotMarkAsReceived"),
            null,
            async response => {
                const updatedOrder = await response.json() as Models.Shop.Order;
                onChange(updatedOrder);
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    return (
    <>
        <Table>
            <thead>
                <tr>
                    <th></th>
                    <th>{resolveText("Order_CreationTimestamp")}</th>
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
                        <td>{formatDateTime(new Date(order.statusChanges[0].timestamp))}</td>
                        <td>
                            {order.items.map(item => (
                                <div>
                                    {item.amount}x {item.product.name}
                                </div>
                            ))}
                        </td>
                        <td>
                            <span className={
                                order.status === OrderStatus.Cancelled ? 'text-secondary'
                                : order.status === OrderStatus.Received ? 'text-success'
                                : order.status === OrderStatus.Returned ? 'text-danger'
                                : 'text-dark'
                            }>
                                <strong>{resolveText(`OrderStatus_${order.status}`)}</strong>
                            </span>
                        </td>
                        <td>
                            {order.status === OrderStatus.Placed
                            ? <AsyncButton
                                onClick={() => cancel(order.id)}
                                variant="danger"
                                isExecuting={isSubmitting}
                                activeText={<><i className="fa fa-times" /> {resolveText("Cancel")}</>}
                                executingText={resolveText("Submitting...")}
                            /> : null}
                            {order.status === OrderStatus.Shipped
                            ? <AsyncButton
                                onClick={() => markAsReceived(order.id)}
                                variant="success"
                                isExecuting={isSubmitting}
                                activeText={<><i className="fa fa-check" /> {resolveText("OrderStatus_Received")}</>}
                                executingText={resolveText("Submitting...")}
                            /> : null}
                        </td>
                        <td>
                            <Button
                                onClick={() => openOrder(order)}
                            >
                                {resolveText("Open")}
                            </Button>
                        </td>
                    </tr>))
                }
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