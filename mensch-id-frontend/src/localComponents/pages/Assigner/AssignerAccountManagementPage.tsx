import { useContext, useEffect } from "react";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import UserContext from "../../contexts/UserContext";
import { ContactInformationForm } from "../../components/Assigner/ContactInformationForm";
import { ChangePasswordForm } from "../../components/ChangePasswordForm";
import { ViewModels } from "../../types/viewModels";
import { AssignerNameForm } from "../../components/Assigner/AssignerNameForm";
import { AssignerLogoUploadForm } from "../../components/Assigner/AssignerLogoUploadForm";
import { buildLoadObjectFunc } from "../../../sharedCommonComponents/helpers/LoadingHelpers";

interface AssignerAccountManagementPageProps {
    setUserViewModel: (userViewModel: ViewModels.AssignerAccountViewModel) => void;
}

export const AssignerAccountManagementPage = (props: AssignerAccountManagementPageProps) => {

    const { setUserViewModel } = props;
    const user = useContext(UserContext)! as ViewModels.AssignerAccountViewModel;

    useEffect(() => {
        if(user) {
            return;
        }
        const loadAssignerProfile = buildLoadObjectFunc(
            'api/assigner/me', {},
            resolveText("Assigner_CouldNotLoadProfile"),
            setUserViewModel
        );
        loadAssignerProfile();
    }, []);

    return (
    <>
        <h1>{resolveText("AccountManagement")}</h1>

        <h3>{resolveText("AssignerAccount_Name")}</h3>
        <AssignerNameForm 
            setUserViewModel={setUserViewModel}
        />
        
        <hr className="my-3" />

        <h3>{resolveText("ContactInformation")}</h3>
        <ContactInformationForm
            setUserViewModel={setUserViewModel}
        />

        <hr className="my-3" />

        <AssignerLogoUploadForm 
            onAccountChanged={setUserViewModel}
        />

        <hr className="my-3" />
        
        <h3>{resolveText("ChangePassword")}</h3>
        <ChangePasswordForm
            accountId={user.accountId}
        />
    </>);

}
export default AssignerAccountManagementPage;