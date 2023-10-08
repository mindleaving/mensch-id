import { Col, FormCheck, Row } from "react-bootstrap";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { Models } from "../../types/models";
import { useMemo, useState } from "react";
import { ContactFormSection } from "./ContactFormSection";
import { PreviousNextButtonRow } from "./PreviousNextButtonRow";

interface ContactInformationShopStepProps {
    order: Models.Shop.Order;
    onChange: (update: Update<Models.Shop.Order>) => void;
    onPrevious: () => void;
    onNext: () => void;
}

export const ContactInformationShopStep = (props: ContactInformationShopStepProps) => {

    const { order, onChange, onPrevious, onNext } = props;
    const [ useInvoiceAddressForShipping, setuUseInvoiceAddressForShipping ] = useState<boolean>(true);
    const isInvoiceAddressComplete = useMemo(() => {
        const address = order.invoiceAddress;
        return address?.name?.trim().length > 0
            && address?.address.street?.trim().length > 0
            && address?.address.postalCode?.trim().length > 0
            && address?.address.city?.trim().length > 0
            && address?.address.country?.trim().length > 0
    }, [ order ]);

    return (
    <>
        <h1>{resolveText("Shop")}</h1>
        <h3>{resolveText("ContactInformation")}</h3>
        <Row>
            <Col lg>
                <h5>{resolveText("Order_InvoiceAddress")}</h5>
                <ContactFormSection
                    required
                    requireEmail
                    value={order.invoiceAddress}
                    onChange={update => onChange(state => ({
                        ...state,
                        invoiceAddress: update(state.invoiceAddress)
                    }))}
                />
            </Col>
            <Col lg>
                <h5>{resolveText("Order_ShippingAddress")}</h5>
                <FormCheck
                    label={resolveText("Order_UseInvoiceAddressForShipping")}
                    checked={useInvoiceAddressForShipping}
                    onChange={e => setuUseInvoiceAddressForShipping(e.target.checked)}
                />
                {useInvoiceAddressForShipping
                ? null
                : <ContactFormSection
                    required
                    value={order.shippingAddress}
                    onChange={update => onChange(state => ({
                        ...state,
                        shippingAddress: update(state.shippingAddress)
                    }))}
                />}
            </Col>
        </Row>
        <PreviousNextButtonRow
            onPrevious={onPrevious}
            onNext={onNext}
            canMoveNext={isInvoiceAddressComplete}
        />
    </>);

}