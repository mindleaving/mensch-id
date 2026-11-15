import { ComponentProps } from 'react';
import { ActivityIndicator, Pressable, Text } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { CommonStyles } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';

interface AppButtonProps {
    title: string;
    onPress: () => void;
    icon?: ComponentProps<typeof MaterialCommunityIcons>['name'];
    variant?: 'primary' | 'outline' | 'danger';
    disabled?: boolean;
    isLoading?: boolean;
    testID?: string;
}

export const AppButton = (props: AppButtonProps) => {

    const colors = useThemeColors();
    const variant = props.variant ?? 'primary';
    const isDisabled = props.disabled || props.isLoading;
    const backgroundColor = variant === 'primary' ? colors.primary
        : variant === 'danger' ? colors.danger
        : 'transparent';
    const foregroundColor = variant === 'outline' ? colors.primary : colors.onPrimary;

    return (
        <Pressable
            accessibilityRole="button"
            accessibilityLabel={props.title}
            testID={props.testID}
            onPress={props.onPress}
            disabled={isDisabled}
            style={({ pressed }) => [
                CommonStyles.button,
                {
                    backgroundColor: backgroundColor,
                    borderColor: colors.primary,
                    borderWidth: variant === 'outline' ? 1 : 0,
                    opacity: isDisabled ? 0.5 : pressed ? 0.8 : 1
                }
            ]}
        >
            {props.isLoading
            ? <ActivityIndicator color={foregroundColor} />
            : props.icon
                ? <MaterialCommunityIcons name={props.icon} size={22} color={foregroundColor} />
                : null}
            <Text style={[ CommonStyles.buttonText, { color: foregroundColor } ]}>{props.title}</Text>
        </Pressable>
    );
};
