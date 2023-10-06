import { useContext } from "react";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import UserContext from "../../contexts/UserContext";
import { ContactInformationForm } from "../../components/Assigner/ContactInformationForm";
import { ChangePasswordForm } from "../../components/ChangePasswordForm";
import { ViewModels } from "../../types/viewModels";
import { AssignerNameForm } from "../../components/Assigner/AssignerNameForm";
import { AssignerLogoUploadForm } from "../../components/Assigner/AssignerLogoUploadForm";

interface AssignerAccountManagementPageProps {}

export const AssignerAccountManagementPage = (props: AssignerAccountManagementPageProps) => {

    const user = useContext(UserContext)! as ViewModels.AssignerAccountViewModel;

    return (
    <>
        <h1>{resolveText("AccountManagement")}</h1>

        <h3>{resolveText("AssignerAccount_Name")}</h3>
        <AssignerNameForm />
        
        <hr className="my-3" />

        <h3>{resolveText("ContactInformation")}</h3>
        <ContactInformationForm />

        <AssignerLogoUploadForm />

        <hr className="my-3" />
        
        <h3>{resolveText("ChangePassword")}</h3>
        <ChangePasswordForm
            accountId={user.accountId}
        />
    </>);

}
export default AssignerAccountManagementPage;