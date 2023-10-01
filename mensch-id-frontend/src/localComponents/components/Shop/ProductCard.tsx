import { Button, Card, Col, FormControl, Row } from "react-bootstrap";
import { Models } from "../../types/models";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { formatMoney } from "../../helpers/Formatter";
import { useState } from "react";
import { Center } from "../../../sharedCommonComponents/components/Center";

interface ProductCardProps {
    product: Models.Shop.Product;
    onAddToShoppingCart: (orderItem: Models.Shop.OrderItem) => void;
    isInShoppingCart?: boolean;
}

export const ProductCard = (props: ProductCardProps) => {

    const { product, onAddToShoppingCart, isInShoppingCart } = props;

    const [ amount, setAmount ] = useState<number>(1);

    const addToShoppingCart = () => {
        const orderItem: Models.Shop.OrderItem = {
            product: product,
            amount: amount
        };
        onAddToShoppingCart(orderItem);
        setAmount(1);
    }

    // SECURITY CHECKS
    if(product.imageUrl && !product.imageUrl.startsWith('/')) {
        return null;
    }
    if(product.downloadLink && !product.downloadLink.startsWith('/')) {
        return null;
    }

    return (
        <Card>
            <Card.Header>
                <Card.Title>{product.name}</Card.Title>
            </Card.Header>
            <Card.Body>
                <Center>
                    {product.imageUrl
                    ? <img src={product.imageUrl} alt={product.name} width='80%' />
                    : <div className="border border-solid p-3">
                        No image
                    </div>}
                </Center>
            </Card.Body>
            <Card.Footer>
                {product.downloadLink
                ? <Row className="align-items-center g-2">
                    <Col>
                        <span className="text-nowrap">
                            <strong>{resolveText("Product_Price")}: {formatMoney(product.price)}</strong>
                        </span>
                    </Col>
                    <Col xs="auto">
                        <a
                            href={product.downloadLink!}
                            download
                            className="btn btn-primary"
                        >
                            {resolveText("Download")}
                        </a>
                    </Col>
                </Row>
                : <Row className="align-items-center g-2">
                    <Col>
                        <span className="text-nowrap">
                            <strong>{resolveText("Product_Price")}: {formatMoney(product.price)}</strong>
                        </span>
                    </Col>
                    <Col xs="auto">
                        <FormControl
                            type="number"
                            value={amount}
                            onChange={e => setAmount(Number(e.target.value))}
                            min={0}
                        />
                    </Col>
                    <Col xs="auto">
                        {isInShoppingCart
                        ? <Button
                            disabled
                            variant="success"
                        >
                            <i className="fa fa-check" /> <i className="fa fa-shopping-cart" />
                        </Button>
                        : <Button
                            onClick={addToShoppingCart}
                            disabled={amount === 0}
                        >
                            <i className="fa fa-shopping-cart" />
                        </Button>}
                    </Col>
                </Row>}
            </Card.Footer>
        </Card>
    );

}