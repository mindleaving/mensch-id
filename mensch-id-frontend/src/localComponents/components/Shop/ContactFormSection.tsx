import { RowFormGroup } from "../../../sharedCommonComponents/components/FormControls/RowFormGroup";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { Models } from "../../types/models";
import { AddressFormControl } from "./AddressFormControl";

interface ContactFormSectionProps {
    value: Models.Contact;
    onChange: (update: Update<Models.Contact>) => void;
    required?: boolean;
    requireEmail?: boolean;
    requirePhoneNumber?: boolean;
}

export const ContactFormSection = (props: ContactFormSectionProps) => {

    const { value, onChange, required, requireEmail, requirePhoneNumber } = props;

    return (
    <>
        <RowFormGroup 
            required={required}
            label={resolveText("Contact_Name")}
            value={value.name}
            onChange={name => onChange(state => ({
                ...state,
                name: name
            }))}
        />
        <RowFormGroup
            required={requireEmail}
            label={resolveText("Contact_Email")}
            value={value.email}
            onChange={email => onChange(state => ({
                ...state,
                email: email
            }))}
        />
        <RowFormGroup
            required={requirePhoneNumber}
            label={resolveText("Contact_PhoneNumber")}
            value={value.phoneNumber}
            onChange={phoneNumber => onChange(state => ({
                ...state,
                phoneNumber: phoneNumber
            }))}
        />
        <AddressFormControl 
            required={required}
            value={value.address}
            onChange={update => onChange(state => ({
                ...state,
                address: update(state.address)
            }))}
        />
    </>);

}