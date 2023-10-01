import { Row, Col, Button, Alert } from "react-bootstrap";
import { LoadingAlert } from "../../../sharedCommonComponents/components/LoadingAlert";
import { NoEntriesAlert } from "../../../sharedCommonComponents/components/NoEntriesAlert";
import { UniformGrid } from "../../../sharedCommonComponents/components/UniformGrid";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { ShoppingCartModal } from "../../modals/ShoppingCartModal";
import { ProductCard } from "./ProductCard";
import { Models } from "../../types/models";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { useState, useMemo, useEffect } from "react";
import { groupBy } from "../../../sharedCommonComponents/helpers/CollectionHelpers";
import { buildLoadObjectFunc } from "../../../sharedCommonComponents/helpers/LoadingHelpers";
import { Center } from "../../../sharedCommonComponents/components/Center";
import { OrderStatus } from "../../types/enums.d";
import { useNavigate } from "react-router-dom";

interface ProductSelectionShopStepProps {
    order: Models.Shop.Order;
    onChange: (update: Update<Models.Shop.Order>) => void;
    goToCheckout: () => void;
}

export const ProductSelectionShopStep = (props: ProductSelectionShopStepProps) => {

    const { order, onChange, goToCheckout } = props;

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ openOrders, setOpenOrders ] = useState<Models.Shop.Order[]>([]);
    const [ products, setProducts ] = useState<Models.Shop.Product[]>([]);
    const categoryGroupedProducts = useMemo(() => groupBy(products, x => x.category), [ products ]);
    const [ showShoppingCart, setShowShoppingCart ] = useState<boolean>(false);
    const navigate = useNavigate();

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

    useEffect(() => {
        const loadOpenOrders = buildLoadObjectFunc(
            'api/shop/my-orders', 
            [ 
                { key: 'status', value: OrderStatus.Placed },
                { key: 'status', value: OrderStatus.Accepted },
                { key: 'status', value: OrderStatus.Shipped },
            ],
            resolveText("Orders_CouldNotLoad"),
            setOpenOrders
        );
        loadOpenOrders();
    }, []);

    const addToShoppingCart = (orderItem: Models.Shop.OrderItem) => {
        if(orderItem.amount === 0) {
            return;
        }
        onChange(state => {
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
        onChange(state => ({
            ...state,
            items: state.items.filter(x => x.product.id !== productId)
        }));
    }

    return (
    <>
        <Row>
            <Col>
                <h1>{resolveText("Assigner_Shop")}</h1>
            </Col>
            <Col xs="auto">
                <Button
                    onClick={() => setShowShoppingCart(true)}
                    variant="danger"
                    size="lg"
                    disabled={order.items.length === 0}
                >
                    <i className="fa fa-shopping-cart" /> ({order.items.length})
                </Button>
            </Col>
        </Row>
        {openOrders.length > 0
        ? <Row className="mb-3">
            <Col>
                <h2>{resolveText("Shop_OpenOrders")}</h2>
                <Alert
                    variant="info"
                    className="py-1"
                >
                    {resolveText("Shop_OpenOrdersAlertText").replace('{COUNT}', openOrders.length.toString())}
                    <Button
                        variant="link"
                        onClick={() => navigate('/shop/orders')}
                    >
                        {resolveText("Show")}...
                    </Button>
                </Alert>
            </Col>
        </Row> : null}
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
        <Center className="mt-3">
            <Button
                onClick={goToCheckout}
                size="lg"
                disabled={order.items.length === 0}
            >
                {resolveText("Shop_GoToCheckout")}
            </Button>
        </Center>
        <ShoppingCartModal 
            show={showShoppingCart}
            onClose={() => setShowShoppingCart(false)}
            order={order}
            onChange={onChange}
            goToCheckout={goToCheckout}
        />
    </>);

}