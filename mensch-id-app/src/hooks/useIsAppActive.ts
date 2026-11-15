import { useEffect, useState } from 'react';
import { AppState } from 'react-native';

export const useIsAppActive = (): boolean => {
    const [ isActive, setIsActive ] = useState<boolean>(AppState.currentState === 'active');

    useEffect(() => {
        const subscription = AppState.addEventListener('change', nextAppState => {
            setIsActive(nextAppState === 'active');
        });
        return () => subscription.remove();
    }, []);

    return isActive;
};
