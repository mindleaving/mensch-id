import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';

export const AuthorizeEndpointPath = 'api/oidc/authorize';

export const getValidAuthorizeReturnUrl = (returnUrl: string | null): string | undefined => {
    if(!returnUrl) {
        return undefined;
    }
    const authorizeEndpointUrl = new URL(apiClient.instance!.buildUrl(AuthorizeEndpointPath));
    let parsedReturnUrl: URL;
    try {
        parsedReturnUrl = new URL(returnUrl);
    } catch {
        return undefined;
    }
    if(parsedReturnUrl.origin !== authorizeEndpointUrl.origin || parsedReturnUrl.pathname !== authorizeEndpointUrl.pathname) {
        return undefined;
    }
    return parsedReturnUrl.toString();
};

export const buildExternalLoginUrl = (
    loginProviderId: string,
    redirectUrl: string,
    isLinking: boolean
): string => {
    const encodedProviderId = encodeURIComponent(loginProviderId);
    const encodedRedirectUrl = encodeURIComponent(redirectUrl);
    const linkParameter = isLinking ? '&link=true' : '';
    return apiClient.instance!.buildUrl(`api/accounts/login/${encodedProviderId}?redirectUrl=${encodedRedirectUrl}${linkParameter}`);
};
