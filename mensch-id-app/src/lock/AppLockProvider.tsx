import { PropsWithChildren, useEffect, useRef, useState } from 'react';
import { AppState } from 'react-native';
import { useTranslation } from 'react-i18next';
import { AppLockContext } from './AppLockContext';
import { AppLockService, isLockSuppressed } from './AppLockService';
import { authenticateWithBiometrics, isBiometricsAvailable, nativeAppLockDependencies } from './NativeAppLockDependencies';
import { useAuth } from '../auth/AuthContext';
import { AppLockContextValue, AppLockStatus, AuthStatus, LockTrigger } from '../types/frontendTypes';

interface AppLockProviderProps {}

export const AppLockProvider = (props: PropsWithChildren<AppLockProviderProps>) => {

    const { t } = useTranslation();
    const auth = useAuth();
    const [ appLockService ] = useState(() => new AppLockService(nativeAppLockDependencies));
    const [ status, setStatus ] = useState<AppLockStatus>('loading');
    const [ previousAuthStatus, setPreviousAuthStatus ] = useState<AuthStatus>(auth.status);
    const [ lockTrigger, setLockTrigger ] = useState<LockTrigger>();
    const [ isPinResetRequested, setPinResetRequested ] = useState<boolean>(false);
    const [ isBiometricsHardwareAvailable, setIsBiometricsHardwareAvailable ] = useState<boolean>(false);
    const [ isBiometricsEnabled, setIsBiometricsEnabled ] = useState<boolean>(false);
    const [ remainingPinAttempts, setRemainingPinAttempts ] = useState<number>(0);
    const activeSuppressionCount = useRef<number>(0);
    const lastSuppressionEndTimestamp = useRef<number>(0);

    if(auth.status !== previousAuthStatus) {
        setPreviousAuthStatus(auth.status);
        if(auth.status === 'loggedIn') {
            setStatus('loading');
            setLockTrigger(previousAuthStatus === 'loggedOut' ? 'freshLogin' : 'coldStart');
        } else {
            setLockTrigger(undefined);
        }
    }

    useEffect(() => {
        const subscription = AppState.addEventListener('change', nextAppState => {
            if(nextAppState !== 'background') {
                return;
            }
            if(isLockSuppressed(activeSuppressionCount.current, lastSuppressionEndTimestamp.current, Date.now())) {
                return;
            }
            setStatus(currentStatus => currentStatus === 'unlocked' ? 'locked' : currentStatus);
        });
        return () => subscription.remove();
    }, []);

    const runWithoutLock = async <T,>(action: () => Promise<T>): Promise<T> => {
        activeSuppressionCount.current += 1;
        try {
            return await action();
        } finally {
            activeSuppressionCount.current -= 1;
            lastSuppressionEndTimestamp.current = Date.now();
        }
    };

    useEffect(() => {
        const checkBiometrics = async () => {
            const isAvailable = await isBiometricsAvailable().catch(() => false);
            setIsBiometricsHardwareAvailable(isAvailable);
        };
        checkBiometrics();
    }, []);

    useEffect(() => {
        if(!lockTrigger) {
            return;
        }
        let isCancelled = false;
        const determineLockStatus = async () => {
            if(lockTrigger === 'freshLogin' && isPinResetRequested) {
                await appLockService.reset();
            }
            const hasPin = await appLockService.hasPin();
            const isEnabled = await appLockService.isBiometricsEnabled();
            const remainingAttempts = await appLockService.getRemainingPinAttempts();
            if(isCancelled) {
                return;
            }
            setPinResetRequested(false);
            setIsBiometricsEnabled(isEnabled);
            setRemainingPinAttempts(remainingAttempts);
            if(!hasPin) {
                setStatus('setupRequired');
                return;
            }
            setStatus(lockTrigger === 'coldStart' ? 'locked' : 'unlocked');
        };
        determineLockStatus();
        return () => {
            isCancelled = true;
        };
    }, [ lockTrigger, isPinResetRequested, appLockService ]);

    const setInitialPin = async (pin: string) => {
        await appLockService.setPin(pin);
        setRemainingPinAttempts(await appLockService.getRemainingPinAttempts());
        setStatus('unlocked');
    };

    const unlockWithPin = async (pin: string) => {
        const result = await appLockService.verifyPin(pin);
        setRemainingPinAttempts(await appLockService.getRemainingPinAttempts());
        if(result === 'valid') {
            setStatus('unlocked');
        }
        if(result === 'lockedOut') {
            setIsBiometricsEnabled(false);
            await auth.logout();
        }
        return result;
    };

    const unlockWithBiometrics = async () => {
        const isAuthenticated = await runWithoutLock(() => authenticateWithBiometrics(t('Lock_BiometricsPrompt'), t('Lock_UsePin')));
        if(!isAuthenticated) {
            return 'failed' as const;
        }
        const isPinRequired = await appLockService.recordBiometricUnlock();
        if(isPinRequired) {
            return 'pinRequired' as const;
        }
        setStatus('unlocked');
        return 'unlocked' as const;
    };

    const changePin = async (currentPin: string, newPin: string) => {
        const result = await appLockService.changePin(currentPin, newPin);
        setRemainingPinAttempts(await appLockService.getRemainingPinAttempts());
        if(result === 'lockedOut') {
            setIsBiometricsEnabled(false);
            await auth.logout();
        }
        return result;
    };

    const setBiometricsEnabled = async (isEnabled: boolean) => {
        if(isEnabled) {
            const isAuthenticated = await runWithoutLock(() => authenticateWithBiometrics(t('Lock_BiometricsEnablePrompt'), t('Cancel')));
            if(!isAuthenticated) {
                return false;
            }
        }
        await appLockService.setBiometricsEnabled(isEnabled);
        setIsBiometricsEnabled(isEnabled);
        return true;
    };

    const value: AppLockContextValue = {
        status: status,
        isBiometricsAvailable: isBiometricsHardwareAvailable,
        isBiometricsEnabled: isBiometricsEnabled && isBiometricsHardwareAvailable,
        remainingPinAttempts: remainingPinAttempts,
        setInitialPin: setInitialPin,
        unlockWithPin: unlockWithPin,
        unlockWithBiometrics: unlockWithBiometrics,
        changePin: changePin,
        setBiometricsEnabled: setBiometricsEnabled,
        setPinResetRequested: setPinResetRequested,
        runWithoutLock: runWithoutLock
    };

    return (
        <AppLockContext.Provider value={value}>
            {props.children}
        </AppLockContext.Provider>
    );
};
