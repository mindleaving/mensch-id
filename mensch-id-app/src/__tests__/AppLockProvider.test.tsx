import '../i18n';
import { act, render } from '@testing-library/react-native';
import { AppState, AppStateStatus } from 'react-native';
import { AuthContext } from '../auth/AuthContext';
import { AppLockProvider } from '../lock/AppLockProvider';
import { useAppLock } from '../lock/AppLockContext';
import { isLockSuppressed, LockSuppressionGraceMilliseconds } from '../lock/AppLockService';
import { AppLockContextValue, AppLockSettings, AuthContextValue, AuthStatus } from '../types/frontendTypes';

let mockStoredSettings: AppLockSettings = { isBiometricsEnabled: false, failedPinAttempts: 0, biometricUseCount: 0 };
let mockResolveBiometrics: (isSuccess: boolean) => void = () => undefined;

jest.mock('../lock/NativeAppLockDependencies', () => ({
    nativeAppLockDependencies: {
        storage: {
            load: async () => ({ ...mockStoredSettings }),
            save: async (settings: AppLockSettings) => {
                mockStoredSettings = { ...settings };
            }
        },
        hashPin: async (pin: string, salt: string) => `hash(${salt}:${pin})`,
        createSalt: async () => 'salt',
        now: () => Date.now()
    },
    isBiometricsAvailable: async () => true,
    authenticateWithBiometrics: () => new Promise<boolean>(resolve => {
        mockResolveBiometrics = resolve;
    })
}));

const createAuthValue = (status: AuthStatus): AuthContextValue => ({
    status: status,
    isLoginAvailable: true,
    apiClient: {} as AuthContextValue['apiClient'],
    login: jest.fn(),
    logout: jest.fn(),
    retryConnection: jest.fn()
});

describe('isLockSuppressed', () => {
    it('suppresses while an expected system dialog is open', () => {
        expect(isLockSuppressed(1, 0, 10_000)).toBe(true);
    });

    it('suppresses shortly after an expected system dialog closed', () => {
        expect(isLockSuppressed(0, 10_000, 10_000 + LockSuppressionGraceMilliseconds - 1)).toBe(true);
    });

    it('does not suppress otherwise', () => {
        expect(isLockSuppressed(0, 10_000, 10_000 + LockSuppressionGraceMilliseconds)).toBe(false);
        expect(isLockSuppressed(0, 0, 1_000_000)).toBe(false);
    });
});

describe('AppLockProvider', () => {
    let appStateListener: ((state: AppStateStatus) => void) | undefined;
    let latestValue: AppLockContextValue | undefined;

    const Probe = () => {
        latestValue = useAppLock();
        return null;
    };

    const renderProvider = async (authStatus: AuthStatus) => {
        return await render(
            <AuthContext.Provider value={createAuthValue(authStatus)}>
                <AppLockProvider>
                    <Probe />
                </AppLockProvider>
            </AuthContext.Provider>
        );
    };

    const rerenderProvider = async (rerender: (element: React.ReactElement) => Promise<void> | void, authStatus: AuthStatus) => {
        await act(async () => {
            await rerender(
                <AuthContext.Provider value={createAuthValue(authStatus)}>
                    <AppLockProvider>
                        <Probe />
                    </AppLockProvider>
                </AuthContext.Provider>
            );
        });
    };

    const emitAppState = async (state: AppStateStatus) => {
        await act(async () => {
            appStateListener?.(state);
        });
    };

    const startUnlockedSession = async () => {
        const { rerender } = await renderProvider('loggedOut');
        await rerenderProvider(rerender, 'loggedIn');
        expect(latestValue!.status).toBe('unlocked');
    };

    beforeEach(() => {
        mockStoredSettings = {
            pinHash: 'hash(salt:123456)',
            pinSalt: 'salt',
            isBiometricsEnabled: false,
            failedPinAttempts: 0,
            biometricUseCount: 0,
            lastPinEntryTimestamp: Date.now()
        };
        appStateListener = undefined;
        jest.spyOn(AppState, 'addEventListener').mockImplementation((_type, listener) => {
            appStateListener = listener as (state: AppStateStatus) => void;
            return { remove: jest.fn() } as unknown as ReturnType<typeof AppState.addEventListener>;
        });
    });

    afterEach(() => {
        jest.restoreAllMocks();
    });

    it('locks on cold start when a PIN exists', async () => {
        const { rerender } = await renderProvider('loading');

        await rerenderProvider(rerender, 'loggedIn');

        expect(latestValue!.status).toBe('locked');
    });

    it('does not lock right after a fresh login', async () => {
        await startUnlockedSession();
    });

    it('requires a PIN setup after a fresh login without PIN', async () => {
        mockStoredSettings = { isBiometricsEnabled: false, failedPinAttempts: 0, biometricUseCount: 0 };
        const { rerender } = await renderProvider('loggedOut');

        await rerenderProvider(rerender, 'loggedIn');

        expect(latestValue!.status).toBe('setupRequired');
    });

    it('locks when the app goes to the background', async () => {
        await startUnlockedSession();

        await emitAppState('background');

        expect(latestValue!.status).toBe('locked');
    });

    it('does not lock when the app only becomes inactive', async () => {
        await startUnlockedSession();

        await emitAppState('inactive');

        expect(latestValue!.status).toBe('unlocked');
    });

    it('does not lock while an expected system dialog is shown', async () => {
        await startUnlockedSession();
        let finishDialog: () => void = () => undefined;
        const dialog = new Promise<void>(resolve => {
            finishDialog = resolve;
        });

        let dialogResult: Promise<void> | undefined;
        await act(async () => {
            dialogResult = latestValue!.runWithoutLock(() => dialog);
        });
        await emitAppState('background');
        await emitAppState('active');
        await act(async () => {
            finishDialog();
            await dialogResult;
        });

        expect(latestValue!.status).toBe('unlocked');
    });

    it('locks again on a later background transition after the grace period', async () => {
        await startUnlockedSession();
        await act(async () => {
            await latestValue!.runWithoutLock(async () => undefined);
        });
        const now = Date.now();
        jest.spyOn(Date, 'now').mockReturnValue(now + LockSuppressionGraceMilliseconds + 1);

        await emitAppState('background');

        expect(latestValue!.status).toBe('locked');
    });

    it('unlocks with the correct PIN after locking', async () => {
        await startUnlockedSession();
        await emitAppState('background');

        let result: string | undefined;
        await act(async () => {
            result = await latestValue!.unlockWithPin('123456');
        });

        expect(result).toBe('valid');
        expect(latestValue!.status).toBe('unlocked');
    });

    it('does not lock during the biometric prompt itself', async () => {
        mockStoredSettings.isBiometricsEnabled = true;
        const { rerender } = await renderProvider('loading');
        await rerenderProvider(rerender, 'loggedIn');
        expect(latestValue!.status).toBe('locked');

        let unlockResult: Promise<string> | undefined;
        await act(async () => {
            unlockResult = latestValue!.unlockWithBiometrics();
        });
        await emitAppState('background');
        await act(async () => {
            mockResolveBiometrics(true);
            await unlockResult;
        });

        expect(latestValue!.status).toBe('unlocked');
    });
});
