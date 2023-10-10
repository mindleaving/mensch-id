import { Button, Modal } from "react-bootstrap";
import { resolveText } from "../../sharedCommonComponents/helpers/Globalizer";
import { ShoppingCartTable } from "../components/Shop/ShoppingCartTable";
import { Models } from "../types/models";
import { Update } from "../../sharedCommonComponents/types/frontendTypes";

interface ShoppingCartModalProps {
    show: boolean;
    onClose: () => void;
    order: Models.Shop.Order;
    onChange: (update: Update<Models.Shop.Order>) => void;
    goToCheckout: () => void;
}

export const ShoppingCartModal = (props: ShoppingCartModalProps) => {

    const { show, onClose, order, onChange, goToCheckout } = props;

    return (
    <Modal show={show} onHide={onClose} size="lg">
        <Modal.Header closeButton>
            <Modal.Title>{resolveText("Shop_ShoppingCart")}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
            <ShoppingCartTable
                order={order}
                onChange={onChange}
            />
        </Modal.Body>
        <Modal.Footer>
            <Button
                onClick={onClose}
                variant="secondary"
            >
                {resolveText("Close")}
            </Button>
            <Button
                onClick={goToCheckout}
            >
                {resolveText("Shop_GoToCheckout")}
            </Button>
        </Modal.Footer>
    </Modal>
    );

}