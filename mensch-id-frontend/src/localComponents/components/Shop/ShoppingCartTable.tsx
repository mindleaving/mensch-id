import { FormControl, Table } from "react-bootstrap";
import { NoEntriesTableRow } from "../../../sharedCommonComponents/components/NoEntriesTableRow";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { formatMoney } from "../../helpers/Formatter";
import { Models } from "../../types/models";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";

interface ShoppingCartTableProps {
    order: Models.Shop.Order;
    onChange: (update: Update<Models.Shop.Order>) => void;
}

export const ShoppingCartTable = (props: ShoppingCartTableProps) => {

    const { order, onChange } = props;

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
                        <i 
                            className="fa fa-trash red clickable"
                            onClick={() => removeFromShoppingCart(orderItem.product.id)}
                        />
                    </td>
                    <td>{orderItem.product.name}</td>
                    <td>{formatMoney(orderItem.product.price)}</td>
                    <td>
                        <FormControl
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
                        />
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
        </tbody>
    </Table>);

}