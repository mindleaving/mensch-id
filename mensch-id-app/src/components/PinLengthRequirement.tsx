import { Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';
import { MinimumPinLength } from '../lock/AppLockService';

interface PinLengthRequirementProps {
    isMet: boolean;
}

const RowHeight = 44;
const IconSize = 18;

export const PinLengthRequirement = (props: PinLengthRequirementProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const color = props.isMet ? colors.mutedText : colors.danger;

    return (
        <View
            testID="pin-length-requirement"
            accessibilityState={{ checked: props.isMet }}
            style={{ height: RowHeight, flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: Spacing.small }}
        >
            <View style={{ width: IconSize, height: IconSize }}>
                {!props.isMet
                ? <MaterialCommunityIcons testID="pin-length-warning" name="alert" size={IconSize} color={colors.danger} />
                : null}
            </View>
            <Text
                numberOfLines={2}
                style={{ fontSize: 14, lineHeight: 18, color: color, textAlign: 'center', flexShrink: 1 }}
            >
                {t('Pin_MinimumLength', { count: MinimumPinLength })}
            </Text>
            <View style={{ width: IconSize, height: IconSize }} />
        </View>
    );
};
