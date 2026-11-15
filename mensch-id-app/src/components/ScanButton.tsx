import { Pressable, Text } from 'react-native';
import { useTranslation } from 'react-i18next';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';

interface ScanButtonProps {
    onPress: () => void;
    testID?: string;
}

export const ScanButton = (props: ScanButtonProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();

    return (
        <Pressable
            accessibilityRole="button"
            accessibilityLabel={t('ScanQrCode')}
            testID={props.testID}
            onPress={props.onPress}
            style={({ pressed }) => ({
                width: 220,
                height: 200,
                borderRadius: 16,
                backgroundColor: colors.primary,
                alignItems: 'center',
                justifyContent: 'center',
                gap: Spacing.small,
                opacity: pressed ? 0.8 : 1
            })}
        >
            <Text style={{ fontSize: 18, fontWeight: '600', color: colors.onPrimary, textAlign: 'center' }}>
                {t('Scan_Title')}
            </Text>
            <MaterialCommunityIcons name="qrcode-scan" size={110} color={colors.onPrimary} />
        </Pressable>
    );
};
