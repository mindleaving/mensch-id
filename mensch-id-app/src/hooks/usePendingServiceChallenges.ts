import { useCallback, useEffect, useState } from 'react';
import { useFocusEffect } from 'expo-router';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useAuth } from '../auth/AuthContext';
import {
    addOrReplaceServiceChallenge,
    ServiceChallengeCreatedMethodName,
    ServiceChallengeHubPath
} from '../helpers/ServiceChallengeHelpers';
import { useIsAppActive } from './useIsAppActive';
import { PendingServiceChallengesState } from '../types/frontendTypes';
import { ViewModels } from '../types/viewModels';

export const usePendingServiceChallenges = (isEnabled: boolean): PendingServiceChallengesState => {

    const auth = useAuth();
    const isAppActive = useIsAppActive();
    const [ state, setState ] = useState<PendingServiceChallengesState>({ challenges: [], hasError: false });

    const loadChallenges = useCallback(async () => {
        try {
            const challenges = await auth.apiClient.get<ViewModels.ServiceChallengeViewModel[]>('api/service-challenges');
            setState({ challenges: challenges, hasError: false });
        } catch {
            setState(previousState => ({ challenges: previousState.challenges, hasError: true }));
        }
    }, [ auth.apiClient ]);

    useFocusEffect(useCallback(() => {
        if(isEnabled) {
            loadChallenges();
        }
    }, [ isEnabled, loadChallenges ]));

    useEffect(() => {
        if(!isEnabled || !isAppActive) {
            return;
        }
        const connection = new HubConnectionBuilder()
            .withUrl(auth.apiClient.getUrl(ServiceChallengeHubPath), { accessTokenFactory: auth.apiClient.getAccessToken })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.None)
            .build();
        connection.on(ServiceChallengeCreatedMethodName, (challenge: ViewModels.ServiceChallengeViewModel) => {
            setState(previousState => ({
                challenges: addOrReplaceServiceChallenge(previousState.challenges, challenge),
                hasError: previousState.hasError
            }));
        });
        connection.onreconnected(() => {
            loadChallenges();
        });
        connection.start()
            .then(loadChallenges)
            .catch(() => undefined);
        return () => {
            connection.stop().catch(() => undefined);
        };
    }, [ isEnabled, isAppActive, auth.apiClient, loadChallenges ]);

    return state;
};
