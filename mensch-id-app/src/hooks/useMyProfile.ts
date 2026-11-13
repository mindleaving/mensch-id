import { useEffect, useState } from 'react';
import { ApiError } from '../communication/ApiClient';
import { useAuth } from '../auth/AuthContext';
import { ViewModels } from '../types/viewModels';
import { ProfileState } from '../types/frontendTypes';

export const useMyProfile = (): ProfileState => {
    const { apiClient } = useAuth();
    const [ profileState, setProfileState ] = useState<ProfileState>({ status: 'loading' });

    useEffect(() => {
        let isCancelled = false;
        const loadProfile = async () => {
            try {
                const profile = await apiClient.get<ViewModels.ProfileViewModel>('api/profiles/me');
                if(!isCancelled) {
                    setProfileState({ status: 'loaded', menschId: profile.id });
                }
            } catch (error) {
                if(isCancelled) {
                    return;
                }
                const isMissingProfile = error instanceof ApiError && error.statusCode === 404;
                setProfileState({ status: isMissingProfile ? 'noProfile' : 'error' });
            }
        };
        loadProfile();
        return () => {
            isCancelled = true;
        };
    }, [ apiClient ]);

    return profileState;
};
