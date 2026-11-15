import { createContext, useContext } from 'react';
import { AppLockContextValue } from '../types/frontendTypes';

export const AppLockContext = createContext<AppLockContextValue | undefined>(undefined);

export const useAppLock = (): AppLockContextValue => {
    const value = useContext(AppLockContext);
    if(!value) {
        throw new Error('useAppLock must be used within an AppLockProvider');
    }
    return value;
};
