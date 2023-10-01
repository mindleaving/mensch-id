import { useState, useMemo, useContext, useEffect } from "react";
import { resolveText } from "../../sharedCommonComponents/helpers/Globalizer";
import { Models } from "../types/models";
import { groupBy } from "../../sharedCommonComponents/helpers/CollectionHelpers";
import { UniformGrid } from "../../sharedCommonComponents/components/UniformGrid";
import { ProductCard } from "../components/Shop/ProductCard";
import { Button, Col, Row } from "react-bootstrap";
import { uuid } from "../../sharedCommonComponents/helpers/uuid";
import { OrderStatus, PaymentMethod, ShippingMethod } from "../types/enums.d";
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
    const [ order, setOrder ] = useState<Models.Shop.Order>({
        id: uuid(),
        creationTimestamp: new Date().toISOString() as any,
        items: [],
        status: OrderStatus.Placed,
        orderedByAccountId: user.accountId,
        invoiceAddress: user.contactInformation,
        sendInvoiceSeparately: false,
        shippingAddress: user.contactInformation,
        shippingMethod: ShippingMethod.Standard,
        paymentMethod: PaymentMethod.Invoice,
    });

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
        setOrder(state => {
            if(state.items.some(x => x.product.id === orderItem.product.id)) {
                return state;
            }
            return {
                ...state,
                items: state.items.concat(orderItem)
            };
        });
    }

    const removeFromShoppingCart = (productId: string) => {
        setOrder(state => ({
            ...state,
            items: state.items.filter(x => x.product.id !== productId)
        }));
    }

    const goToCheckout = () => {

    }

    return (
    <>
        <h1>{resolveText("Assigner_Shop")}</h1>
        <Row>
            <Col />
            <Col xs="auto">
                <Button
                    onClick={goToCheckout}
                    variant="danger"
                    size="lg"
                    disabled={order.items.length === 0}
                >
                    <i className="fa fa-shopping-cart" /> ({order.items.length})
                </Button>
            </Col>
        </Row>
        <h2>{resolveText("Products")}</h2>
        {isLoading ? <LoadingAlert />
        : categoryGroupedProducts.length === 0 ? <NoEntriesAlert />
        : categoryGroupedProducts.map(category => (
        <>
            <h3>{category.key}</h3>
            <UniformGrid
                items={category.items.map(product => {
                    const isInShoppingCart = order.items.some(x => x.product.id === product.id);
                    return (<ProductCard 
                        key={product.id}
                        product={product}
                        isInShoppingCart={isInShoppingCart}
                        onAddToShoppingCart={addToShoppingCart}
                    />);
                })}
                size="md"
                columnCount={2}
            />
        </>))}
    </>);

}
export default AssignerShopPage;