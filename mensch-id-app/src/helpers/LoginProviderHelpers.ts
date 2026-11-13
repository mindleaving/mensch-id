import { ComponentProps } from 'react';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { ApiBaseUrl } from './ConfigurationHelpers';
import { ViewModels } from '../types/viewModels';

type IconName = ComponentProps<typeof MaterialCommunityIcons>['name'];

const LoginProviderIcons: { [loginProviderId: string]: IconName } = {
    google: 'google',
    microsoft: 'microsoft',
    apple: 'apple',
    github: 'github',
    gitlab: 'gitlab'
};

export const getLoginProviderIcon = (loginProviderId: string): IconName => {
    return LoginProviderIcons[loginProviderId.toLowerCase()] ?? 'login';
};

export const loadLoginProviders = async (): Promise<ViewModels.LoginProviderViewModel[]> => {
    const response = await fetch(`${ApiBaseUrl}/api/accounts/login-providers`, {
        headers: { Accept: 'application/json' }
    });
    if(!response.ok) {
        throw new Error(`Could not load login providers (${response.status})`);
    }
    return await response.json() as ViewModels.LoginProviderViewModel[];
};
