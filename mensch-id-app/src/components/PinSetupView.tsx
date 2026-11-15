import { useTranslation } from 'react-i18next';
import { SafeAreaView } from 'react-native-safe-area-context';
import { NewPinEntry } from './NewPinEntry';
import { useAppLock } from '../lock/AppLockContext';
import { useThemeColors } from '../hooks/useThemeColors';

interface PinSetupViewProps {}

export const PinSetupView = (props: PinSetupViewProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const appLock = useAppLock();

    return (
        <SafeAreaView style={{ flex: 1, backgroundColor: colors.background }}>
            <NewPinEntry
                title={t('Pin_SetupTitle')}
                description={t('Pin_SetupDescription')}
                onPinChosen={appLock.setInitialPin}
            />
        </SafeAreaView>
    );
};
