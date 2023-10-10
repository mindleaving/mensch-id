import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../../../sharedCommonComponents/communication/ApiClient';
import { showSuccessAlert, showErrorAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';

interface LinkAccountRedirectPageProps {
    onLogOut: () => void;
}

export const LinkAccountRedirectPage = (props: LinkAccountRedirectPageProps) => {

    const { onLogOut } = props;
    const navigate = useNavigate();
    useEffect(() => {
        const finishLinkage = async () => {
            try {
                const response = await apiClient.instance!.post('api/accounts/link/external', null, {}, { handleError: false });
                if(response.ok) {
                    showSuccessAlert(resolveText("LinkAccount_SuccessfullyLinked"));
                    navigate("/me");
                } else {
                    const errorText = await response.text();
                    showErrorAlert(resolveText("LinkAccount_CouldNotLink"), errorText);
                    if(response.status === 403) {
                        // This happens when the other login is tied to another account, 
                        // which might cause confusion of which account is currently active, 
                        // therefore log out.
                        onLogOut();
                    } else {
                        navigate("/me");
                    }
                }
            } catch {
                showErrorAlert(resolveText("LinkAccount_CouldNotLink"));
            }
        };
        finishLinkage();
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
        <h3>{resolveText("LinkingAccounts...")}</h3>
    );

}
export default LinkAccountRedirectPage;