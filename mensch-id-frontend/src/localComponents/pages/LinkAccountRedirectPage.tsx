import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { NotificationManager } from 'react-notifications';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';

interface LinkAccountRedirectPageProps {}

export const LinkAccountRedirectPage = (props: LinkAccountRedirectPageProps) => {

    const navigate = useNavigate();
    useEffect(() => {
        const finishLinkage = async () => {
            try {
                if(!apiClient.instance!.accessToken) {
                    throw new Error("No access token set");
                }
                await apiClient.instance!.post('api/accounts/link', {}, null);
                NotificationManager.success(resolveText("LinkAccount_SuccessfullyLinked"));
            } catch {
                NotificationManager.error(resolveText("LinkAccount_CouldNotLink"));
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