import { Pressable, Text } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';

interface CheckboxProps {
    label: string;
    isChecked: boolean;
    onChange: (isChecked: boolean) => void;
    testID?: string;
}

export const Checkbox = (props: CheckboxProps) => {

    const colors = useThemeColors();

    return (
        <Pressable
            accessibilityRole="checkbox"
            accessibilityState={{ checked: props.isChecked }}
            accessibilityLabel={props.label}
            testID={props.testID}
            onPress={() => props.onChange(!props.isChecked)}
            style={{ flexDirection: 'row', alignItems: 'center', gap: Spacing.small, paddingVertical: Spacing.small }}
        >
            <MaterialCommunityIcons
                name={props.isChecked ? 'checkbox-marked' : 'checkbox-blank-outline'}
                size={24}
                color={props.isChecked ? colors.primary : colors.mutedText}
            />
            <Text style={{ fontSize: 16, color: colors.text }}>{props.label}</Text>
        </Pressable>
    );
};
