import { Button, Card, Col, FormControl, Row } from "react-bootstrap";
import { Models } from "../../types/models";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { formatMoney } from "../../helpers/Formatter";
import { useState } from "react";

interface ProductCardProps {
    product: Models.Shop.Product;
    onAddToShoppingCart: (orderItem: Models.Shop.OrderItem) => void;
    isInShoppingCart?: boolean;
}

export const ProductCard = (props: ProductCardProps) => {

    const { product, onAddToShoppingCart, isInShoppingCart } = props;

    const [ amount, setAmount ] = useState<number>(0);

    const addToShoppingCart = () => {
        const orderItem: Models.Shop.OrderItem = {
            product: product,
            amount: amount
        };
        onAddToShoppingCart(orderItem);
        setAmount(1);
    }

    return (
        <Card>
            <Card.Header>
                <Card.Title>{product.name}</Card.Title>
            </Card.Header>
            <Card.Body>

            </Card.Body>
            <Card.Footer>
                <Row>
                    <Col>
                        {resolveText("Product_Price")}: {formatMoney(product.price)}
                    </Col>
                    <Col xs="auto">
                        <FormControl
                            type="number"
                            value={amount}
                            onChange={e => setAmount(Number(e.target.value))}
                        />
                    </Col>
                    <Col xs="auto">
                        {isInShoppingCart
                        ? <Button
                            disabled
                            variant="success"
                        >
                            <i className="fa fa-check" /> {resolveText("Shop_IsInShoppingCart")}
                        </Button>
                        : <Button
                            onClick={addToShoppingCart}
                        >
                            {resolveText("Shop_AddToShoppingCart")}
                        </Button>}
                    </Col>
                </Row>
            </Card.Footer>
        </Card>
    );

}