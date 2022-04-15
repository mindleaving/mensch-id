import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { Models } from '../types/models';
import { NotificationManager } from 'react-notifications';

interface LoginRedirectPageProps {
    onLoggedIn: (authenticationResult?: Models.AuthenticationResult) => void;
}

export const LoginRedirectPage = (props: LoginRedirectPageProps) => {

    const { loginProvider } = useParams();
    const navigate = useNavigate();

    useEffect(() => {
        const checkLoginStatus = async () => {
            try {
                const response = await apiClient.instance!.get('api/accounts/is-logged-in', {});
                if(!response.ok) {
                    navigate("/login");
                    return;
                }
                if(loginProvider?.toLowerCase() === "jwt") {
                    const jwtResponse = await apiClient.instance!.get("api/accounts/accesstoken", {});
                    const authenticationResult = await jwtResponse.json() as Models.AuthenticationResult;
                    if(authenticationResult.isAuthenticated) {
                        props.onLoggedIn(authenticationResult);
                    }
                } else {
                    props.onLoggedIn();
                }
            } catch {
                NotificationManager.error(resolveText("CouldNotLogIn"));
                navigate("/login");
            }
        };
        checkLoginStatus(); 
    }, []);
    
    return (
        <h3>{resolveText("LoggingIn....")}</h3>
    );

}