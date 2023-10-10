import { Card, Modal } from "react-bootstrap";
import { Models } from "../types/models";
import { resolveText } from "../../sharedCommonComponents/helpers/Globalizer";
import { ShoppingCartTable } from "../components/Shop/ShoppingCartTable";
import { ContactInformationViewer } from "../components/Shop/ContactInformationViewer";

interface OrderViewerModalProps {
    show: boolean;
    onClose: () => void;
    order: Models.Shop.Order;
}

export const OrderViewerModal = (props: OrderViewerModalProps) => {

    const { show, onClose, order } = props;

    return (
    <Modal show={show} onHide={onClose} size="lg">
        <Modal.Header closeButton>
            <Modal.Title>{resolveText("Order")} {order.id}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
            <Card className="mb-3">
                <Card.Header>
                    <Card.Title>{resolveText("Products")}</Card.Title>
                </Card.Header>
                <Card.Body>
                    <ShoppingCartTable
                        isReadOnly
                        order={order}
                        onChange={_ => {}}
                    />
                </Card.Body>
            </Card>

            <Card>
                <Card.Header>
                    <Card.Title>{resolveText("ContactInformation")}</Card.Title>
                </Card.Header>
                <Card.Body>
                    <div className="mb-3">
                        <h5>{resolveText("Order_PaymentMethod")}: {resolveText(`PaymentMethod_${order.paymentMethod}`)}</h5>
                    </div>
                    <strong>{resolveText("Order_InvoiceAddress")}</strong>
                    <ContactInformationViewer
                        contactInformation={order.invoiceAddress}
                    />

                    <div className="my-3">
                        <h5>{resolveText("Order_ShippingMethod")}: {resolveText(`ShippingMethod_${order.shippingMethod}`)}</h5>
                    </div>
                    <strong>{resolveText("Order_ShippingAddress")}</strong>
                    <ContactInformationViewer
                        contactInformation={order.shippingAddress}
                    />
                </Card.Body>
            </Card>
        </Modal.Body>
    </Modal>);

}