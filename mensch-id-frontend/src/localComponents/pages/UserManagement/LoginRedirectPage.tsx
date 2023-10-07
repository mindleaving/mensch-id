import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../../../sharedCommonComponents/communication/ApiClient';
import { showErrorAlert } from '../../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../../sharedCommonComponents/helpers/Globalizer';
import { AccountType } from '../../types/enums';
import { Models } from '../../types/models';

interface LoginRedirectPageProps {
    onLoggedIn: (isLoggedInResponse: Models.IsLoggedInResponse) => void;
}

export const LoginRedirectPage = (props: LoginRedirectPageProps) => {

    const navigate = useNavigate();

    useEffect(() => {
        const checkLoginStatus = async () => {
            try {
                const response = await apiClient.instance!.get('api/accounts/is-logged-in', {});
                if(!response.ok) {
                    navigate("/login");
                    return;
                }
                let isLoggedInResponse = await response.json() as Models.IsLoggedInResponse;
                if(isLoggedInResponse.accountType !== AccountType.External) {
                    navigate("/login");
                    return;
                }
                const jwtResponse = await apiClient.instance!.get("api/accounts/convert-to-local", {});
                isLoggedInResponse = await jwtResponse.json() as Models.IsLoggedInResponse;
                props.onLoggedIn(isLoggedInResponse);
            } catch {
                showErrorAlert(resolveText("CouldNotLogIn"));
                navigate("/login");
            }
        };
        checkLoginStatus(); 
    }, []);
    
    return (
        <h3>{resolveText("LoggingIn....")}</h3>
    );

}
export default LoginRedirectPage;