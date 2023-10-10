import { FormEvent, useContext, useEffect, useState } from "react";
import UserContext from "../../contexts/UserContext";
import { ViewModels } from "../../types/viewModels";
import { Form } from "react-bootstrap";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { sendPutRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { Models } from "../../types/models";
import { StoreButton } from "../../../sharedCommonComponents/components/StoreButton";
import { Center } from "../../../sharedCommonComponents/components/Center";
import { ContactFormSection } from "../Shop/ContactFormSection";

interface ContactInformationFormProps {
    setUserViewModel: (userViewModel: ViewModels.AssignerAccountViewModel) => void;
}

export const ContactInformationForm = (props: ContactInformationFormProps) => {

    const { setUserViewModel } = props;
    const user = useContext(UserContext)! as ViewModels.AssignerAccountViewModel;

    const [ contactInformation, setContactInformation ] = useState<Models.Contact>(user.contactInformation ?? {
        name: '',
        email: '',
        phoneNumber: '',
        address: {
            street: '',
            postalCode: '',
            city: '',
            country: ''
        }
    });
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ isStored, setIsStored ] = useState<boolean>(true);

    const store = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsSubmitting(true);
        await sendPutRequest(
            `api/assigner/me/contact`, {},
            resolveText("Contact_CouldNotStore"),
            contactInformation,
            async response => {
                setIsStored(true);
                const userViewModel = await response.json() as ViewModels.AssignerAccountViewModel;
                setUserViewModel(userViewModel);
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    useEffect(() => {
        setIsStored(false);
    }, [ contactInformation ]);

    return (<Form onSubmit={store}>
        <ContactFormSection
            value={contactInformation}
            onChange={setContactInformation}
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