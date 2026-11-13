import { ActivityIndicator, Text, View } from 'react-native';
import { AppButton } from './AppButton';
import { CommonStyles, Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';

interface StatusScreenProps {
    message: string;
    isLoading?: boolean;
    actionTitle?: string;
    onAction?: () => void;
}

export const StatusScreen = (props: StatusScreenProps) => {

    const colors = useThemeColors();

    return (
        <View style={[ CommonStyles.screen, CommonStyles.centered, { backgroundColor: colors.background, gap: Spacing.medium } ]}>
            {props.isLoading ? <ActivityIndicator size="large" color={colors.primary} /> : null}
            <Text style={[ CommonStyles.instruction, { color: colors.text } ]}>{props.message}</Text>
            {props.actionTitle && props.onAction
            ? <AppButton title={props.actionTitle} onPress={props.onAction} />
            : null}
        </View>
    );
};
