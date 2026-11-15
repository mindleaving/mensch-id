import { createContext, useContext } from 'react';
import { AuthContextValue } from '../types/frontendTypes';

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const useAuth = (): AuthContextValue => {
    const value = useContext(AuthContext);
    if(!value) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return value;
};
