import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../../../sharedCommonComponents/communication/ApiClient';
import { showSuccessAlert, showErrorAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';

interface LinkAccountRedirectPageProps {}

export const LinkAccountRedirectPage = (props: LinkAccountRedirectPageProps) => {

    const navigate = useNavigate();
    useEffect(() => {
        const finishLinkage = async () => {
            try {
                if(!apiClient.instance!.accessToken) {
                    throw new Error("No access token set");
                }
                await apiClient.instance!.post('api/accounts/link/external', null);
                showSuccessAlert(resolveText("LinkAccount_SuccessfullyLinked"));
            } catch {
                showErrorAlert(resolveText("LinkAccount_CouldNotLink"));
            } finally {
                navigate("/me");
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