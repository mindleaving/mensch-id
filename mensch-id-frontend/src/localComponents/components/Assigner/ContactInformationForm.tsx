import { FormEvent, useContext, useEffect, useState } from "react";
import UserContext from "../../contexts/UserContext";
import { ViewModels } from "../../types/viewModels";
import { Form } from "react-bootstrap";
import { RowFormGroup } from "../../../sharedCommonComponents/components/FormControls/RowFormGroup";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { sendPutRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { Models } from "../../types/models";
import { StoreButton } from "../../../sharedCommonComponents/components/StoreButton";
import { Center } from "../../../sharedCommonComponents/components/Center";

interface ContactInformationFormProps {
}

export const ContactInformationForm = (props: ContactInformationFormProps) => {

    const user = useContext(UserContext)! as ViewModels.AssignerAccountViewModel;

    const [ contactPersonName, setContactPersonName ] = useState<string>(user.contactInformation?.name ?? '');
    const [ email, setEmail ] = useState<string>(user.contactInformation?.email ?? '');
    const [ phoneNumber, setPhoneNumber ] = useState<string>(user.contactInformation?.phoneNumber ?? '');
    const [ street, setStreet ] = useState<string>(user.contactInformation?.address.street ?? '');
    const [ postalCode, setPostalCode ] = useState<string>(user.contactInformation?.address.postalCode ?? '');
    const [ city, setCity ] = useState<string>(user.contactInformation?.address.city ?? '');
    const [ country, setCountry ] = useState<string>(user.contactInformation?.address.country ?? '');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ isStored, setIsStored ] = useState<boolean>(true);

    const store = async (e?: FormEvent) => {
        e?.preventDefault();
        const contactInformation: Models.Contact = {
            name: contactPersonName,
            email: email,
            phoneNumber: phoneNumber,
            address: {
                street: street,
                postalCode: postalCode,
                city: city,
                country: country
            }
        };
        setIsSubmitting(true);
        await sendPutRequest(
            `api/assigner/me/contact`, {},
            resolveText("Contact_CouldNotStore"),
            contactInformation,
            _ => {
                setIsStored(true)
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    useEffect(() => {
        setIsStored(false);
    }, [ contactPersonName, email, phoneNumber, street, postalCode, city, country ]);

    return (<Form onSubmit={store}>
        <RowFormGroup
            label={resolveText("Contact_Name")}
            value={contactPersonName}
            onChange={setContactPersonName}
        />
        <RowFormGroup
            label={resolveText("Contact_Email")}
            value={email}
            onChange={setEmail}
        />
        <RowFormGroup
            label={resolveText("Contact_PhoneNumber")}
            value={phoneNumber}
            onChange={setPhoneNumber}
        />
        <RowFormGroup
            label={resolveText("Address_Street")}
            value={street}
            onChange={setStreet}
        />
        <RowFormGroup
            label={resolveText("Address_PostalCode")}
            value={postalCode}
            onChange={setPostalCode}
        />
        <RowFormGroup
            label={resolveText("Address_City")}
            value={city}
            onChange={setCity}
        />
        <RowFormGroup
            label={resolveText("Address_Country")}
            value={country}
            onChange={setCountry}
        />
        <Center>
            <StoreButton
                type="submit"
                isStoring={isSubmitting}
                isStored={isStored}
            />
        </Center>
    </Form>);

}