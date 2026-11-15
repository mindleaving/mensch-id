import { ViewModels } from '../types/viewModels';

const loginProviderIconLookup: { [loginProviderId: string]: string } = {
    google: 'fa fa-google',
    microsoft: 'fa fa-windows',
    apple: 'fa fa-apple',
    github: 'fa fa-github',
    gitlab: 'fa fa-gitlab',
    local: 'fa fa-user'
};

export const LocalLoginProviderId = 'local';

export const getLoginProviderIconClass = (loginProviderId: string): string | undefined => {
    return loginProviderIconLookup[loginProviderId.toLowerCase()];
};

export const getLoginProviderDisplayName = (
    loginProviders: ViewModels.LoginProviderViewModel[],
    loginProviderId: string
): string => {
    const matchingLoginProvider = loginProviders.find(x => x.id === loginProviderId);
    return matchingLoginProvider?.displayName ?? loginProviderId;
};
