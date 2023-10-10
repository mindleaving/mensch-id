import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { Models } from "../../types/models";
import { RowFormGroup } from "../../../sharedCommonComponents/components/FormControls/RowFormGroup";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";

interface AddressFormControlProps {
    value: Models.Address;
    onChange: (update: Update<Models.Address>) => void;
    required?: boolean;
}

export const AddressFormControl = (props: AddressFormControlProps) => {

    const { value, onChange, required } = props;
    
    return (
    <>
        <RowFormGroup 
            required={required}
            label={resolveText("Address_Street")}
            value={value?.street ?? ''}
            onChange={street => onChange(state => ({
                ...state,
                street: street
            }))}
        />
        <RowFormGroup
            required={required}
            label={resolveText("Address_PostalCode")}
            value={value?.postalCode ?? ''}
            onChange={postalCode => onChange(state => ({
                ...state,
                postalCode: postalCode
            }))}
        />
        <RowFormGroup
            required={required}
            label={resolveText("Address_City")}
            value={value?.city ?? ''}
            onChange={city => onChange(state => ({
                ...state,
                city: city
            }))}
        />
        <RowFormGroup
            required={required}
            label={resolveText("Address_Country")}
            value={value?.country ?? ''}
            onChange={country => onChange(state => ({
                ...state,
                country: country
            }))}
        />
    </>);

}