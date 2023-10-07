import { Form, FormControl, Table } from "react-bootstrap";
import { NoEntriesTableRow } from "../../../sharedCommonComponents/components/NoEntriesTableRow";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { formatMoney } from "../../helpers/Formatter";
import { Models } from "../../types/models";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { useMemo } from "react";
import { Currency } from "../../types/enums";
import { getShipmentCost } from "../../helpers/ShopHelpers";

interface ShoppingCartTableProps {
    isReadOnly?: boolean;
    order: Models.Shop.Order;
    onChange: (update: Update<Models.Shop.Order>) => void;
}

export const ShoppingCartTable = (props: ShoppingCartTableProps) => {

    const { isReadOnly, order, onChange } = props;

    const shipmentCost = useMemo(() => getShipmentCost(order.shippingMethod), [ order ]);
    const total = useMemo((): Models.Shop.Money => {
        const currency = order.items.length > 0 ? order.items[0].product.price.currency : Currency.EUR;
        if(order.items.some(item => item.product.price.currency !== currency)
            || shipmentCost.currency !== currency) {
            return {
                currency: currency,
                value: NaN
            };
        }
        const itemSum = order.items
            .map(item => item.amount * item.product.price.value)
            .reduce((sum, itemTotal) => sum + itemTotal);
        const sum = itemSum + shipmentCost.value;
        return {
            currency: currency,
            value: sum
        };
    }, [ order, shipmentCost ]);

    const removeFromShoppingCart = (productId: string) => {
        onChange(state => ({
            ...state,
            items: state.items.filter(x => x.product.id !== productId)
        }));
    }

    return (
    <Table>
        <thead>
            <tr>
                <th></th>
                <th>{resolveText("Product")}</th>
                <th>{resolveText("Product_Price")}</th>
                <th>{resolveText("OrderItem_Amount")}</th>
                <th>{resolveText("Shop_ShoppingCart_Total")}</th>
            </tr>
        </thead>
        <tbody>
            {order.items.length > 0
            ? order.items.map(orderItem => (
                <tr key={orderItem.product.id}>
                    <td>
                        {isReadOnly
                        ? null
                        : <i 
                            className="fa fa-trash red clickable"
                            onClick={() => removeFromShoppingCart(orderItem.product.id)}
                        />}
                    </td>
                    <td>{orderItem.product.name}</td>
                    <td>{formatMoney(orderItem.product.price)}</td>
                    <td>
                        {isReadOnly
                        ? <Form.Text>{orderItem.amount}</Form.Text>
                        : <FormControl
                            type="number"
                            value={orderItem.amount}
                            onChange={e => onChange(state => ({
                                ...state,
                                items: state.items.map(item => {
                                    if(item.product.id === orderItem.product.id) {
                                        return {
                                            ...item,
                                            amount: Number(e.target.value)
                                        };
                                    }
                                    return item;
                                })
                            }))}
                            min={1}
                            step={1}
                        />}
                    </td>
                    <td>
                        {formatMoney({
                            currency: orderItem.product.price.currency,
                            value: orderItem.amount * orderItem.product.price.value
                        })}
                    </td>
                </tr>
            ))
            : <NoEntriesTableRow colSpan={5} />}
            <tr>
                <td colSpan={3}></td>
                <td>
                    {resolveText("Order_ShippingCost")}
                </td>
                <td>
                    {formatMoney(shipmentCost)}
                </td>
            </tr>
            <tr>
                <td colSpan={3}>
                </td>
                <td>
                    <strong>{resolveText("Shop_ShoppingCart_Total")}</strong>
                </td>
                <td>
                    <strong>{formatMoney(total)}</strong>
                </td>
            </tr>
        </tbody>
    </Table>);

}