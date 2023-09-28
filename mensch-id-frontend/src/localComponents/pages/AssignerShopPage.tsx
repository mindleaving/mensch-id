import { useState, useMemo, FormEvent, useContext, useEffect } from "react";
import { resolveText } from "../../sharedCommonComponents/helpers/Globalizer";
import { Models } from "../types/models";
import { groupBy } from "../../sharedCommonComponents/helpers/CollectionHelpers";
import { UniformGrid } from "../../sharedCommonComponents/components/UniformGrid";
import { ProductCard } from "../components/Shop/ProductCard";
import { Col, Form, FormCheck, Row, Table } from "react-bootstrap";
import { NoEntriesTableRow } from "../../sharedCommonComponents/components/NoEntriesTableRow";
import { formatMoney } from "../helpers/Formatter";
import { AsyncButton } from "../../sharedCommonComponents/components/AsyncButton";
import { sendPostRequest } from "../../sharedCommonComponents/helpers/StoringHelpers";
import { showSuccessAlert } from "../../sharedCommonComponents/helpers/AlertHelpers";
import { uuid } from "../../sharedCommonComponents/helpers/uuid";
import { OrderStatus } from "../types/enums.d";
import UserContext from "../contexts/UserContext";
import { ViewModels } from "../types/viewModels";
import { buildLoadObjectFunc } from "../../sharedCommonComponents/helpers/LoadingHelpers";
import { LoadingAlert } from "../../sharedCommonComponents/components/LoadingAlert";
import { NoEntriesAlert } from "../../sharedCommonComponents/components/NoEntriesAlert";

interface AssignerShopPageProps {}

export const AssignerShopPage = (props: AssignerShopPageProps) => {

    const user = useContext(UserContext) as ViewModels.AssignerAccountViewModel;
    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ products, setProducts ] = useState<Models.Shop.Product[]>([]);
    const categoryGroupedProducts = useMemo(() => groupBy(products, x => x.category), [ products ]);
    const [ shoppingCart, setShoppingCart ] = useState<Models.Shop.OrderItem[]>([]);
    const [ hasAcceptedTermsAndConditions, setHasAcceptedTermsAndConditions ] = useState<boolean>(false);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    useEffect(() => {
        setIsLoading(true);
        const loadProducts = buildLoadObjectFunc(
            'api/shop/products', {},
            resolveText("Products_CouldNotLoad"),
            setProducts,
            undefined,
            () => setIsLoading(false)
        );
        loadProducts();
    }, []);

    const addToShoppingCart = (orderItem: Models.Shop.OrderItem) => {
        if(orderItem.amount === 0) {
            return;
        }
        setShoppingCart(state => {
            if(state.some(x => x.product.id === orderItem.product.id)) {
                return state;
            }
            return state.concat(orderItem);
        });
    }

    const removeFromShoppingCart = (productId: string) => {
        setShoppingCart(state => state.filter(x => x.product.id === productId));
    }

    const submit = async (e?: FormEvent) => {
        e?.preventDefault();
        if(shoppingCart.length === 0) {
            return;
        }
        if(!hasAcceptedTermsAndConditions) {
            return;
        }
        const order: Models.Shop.Order = {
            id: uuid(),
            creationTimestamp: new Date().toISOString() as any,
            items: shoppingCart,
            status: OrderStatus.Placed,
            orderedByAccountId: user.accountId,
            shippingAddress: user.contactInformation.address
        };
        setIsSubmitting(true);
        await sendPostRequest(
            'api/shop', {},
            resolveText("Shop_CouldNotOrder"),
            order,
            () => {
                showSuccessAlert("Order_SuccessfullyPlaced");
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    return (
    <>
        <h1>{resolveText("Assigner_Shop")}</h1>
        <h2>{resolveText("Products")}</h2>
        {isLoading ? <LoadingAlert />
        : categoryGroupedProducts.length === 0 ? <NoEntriesAlert />
        : categoryGroupedProducts.map(category => (
        <>
            <h3>{category.key}</h3>
            <UniformGrid
                items={category.items.map(product => {
                    const isInShoppingCart = shoppingCart.some(x => x.product.id === product.id);
                    return (<ProductCard 
                        product={product}
                        isInShoppingCart={isInShoppingCart}
                        onAddToShoppingCart={addToShoppingCart}
                    />);
                })}
                size="sm"
                columnCount={2}
            />
        </>))}
        <h2>{resolveText("Shop_ShoppingCart")}</h2>
        <Form onSubmit={submit}>
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
                    {shoppingCart.length > 0
                    ? shoppingCart.map(orderItem => (
                        <tr key={orderItem.product.id}>
                            <td>
                                <i 
                                    className="fa fa-trash red clickable"
                                    onClick={() => removeFromShoppingCart(orderItem.product.id)}
                                />
                            </td>
                            <td>{orderItem.product.name}</td>
                            <td>{formatMoney(orderItem.product.price)}</td>
                            <td>{orderItem.amount}</td>
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
            </Table>
            <FormCheck
                label={resolveText("Shop_AccpetTermsAndConditions")}
                checked={hasAcceptedTermsAndConditions}
                onChange={e => setHasAcceptedTermsAndConditions(e.target.checked)}
            />
            <Row>
                <Col />
                <Col xs="auto">
                    <AsyncButton
                        type="submit"
                        isExecuting={isSubmitting}
                        activeText={resolveText("Submit")}
                        executingText={resolveText("Submitting...")}
                        disabled={shoppingCart.length === 0 || !hasAcceptedTermsAndConditions}
                    />
                </Col>
            </Row>
        </Form>
    </>);

}
export default AssignerShopPage;