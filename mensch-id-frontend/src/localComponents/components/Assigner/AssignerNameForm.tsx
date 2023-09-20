import { Form } from "react-bootstrap";
import { RowFormGroup } from "../../../sharedCommonComponents/components/FormControls/RowFormGroup";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { FormEvent, useContext, useEffect, useState } from "react";
import UserContext from "../../contexts/UserContext";
import { ViewModels } from "../../types/viewModels";
import { StoreButton } from "../../../sharedCommonComponents/components/StoreButton";
import { sendPutRequest } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { Center } from "../../../sharedCommonComponents/components/Center";

interface AssignerNameFormProps {}

export const AssignerNameForm = (props: AssignerNameFormProps) => {

    const user = useContext(UserContext)! as ViewModels.AssignerAccountViewModel;

    const [ name, setName ] = useState<string>(user.name ?? '');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ isStored, setIsStored ] = useState<boolean>(true);

    const store = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsSubmitting(true);
        await sendPutRequest(
            `api/assigner/me/name`, {},
            resolveText("AssignerAccount_CouldNotStoreName"),
            `"${name}"`,
            _ => {
                setIsStored(true)
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    useEffect(() => {
        setIsStored(false);
    }, [ name ]);

    return (<Form onSubmit={store}>
        <RowFormGroup required
            label={resolveText("AssignerAccount_Name")}
            value={name}
            onChange={setName}
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