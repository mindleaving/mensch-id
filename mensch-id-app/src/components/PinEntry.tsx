import { ComponentProps, ReactNode, useState } from 'react';
import { ScrollView, Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AppButton } from './AppButton';
import { PinPad } from './PinPad';
import { CommonStyles, Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';
import { MinimumPinLength } from '../lock/AppLockService';

interface PinEntryProps {
    title: string;
    description?: string;
    errorMessage?: string;
    submitTitle?: string;
    onSubmit: (pin: string) => Promise<void>;
    extraKeyIcon?: ComponentProps<typeof MaterialCommunityIcons>['name'];
    extraKeyLabel?: string;
    onExtraKeyPressed?: () => void;
    footer?: ReactNode;
}

export const PinEntry = (props: PinEntryProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const [ pin, setPin ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    const submit = async () => {
        if(pin.length < MinimumPinLength) {
            return;
        }
        setIsSubmitting(true);
        try {
            await props.onSubmit(pin);
        } finally {
            setPin('');
            setIsSubmitting(false);
        }
    };

    return (
        <ScrollView contentContainerStyle={[ CommonStyles.screen, CommonStyles.centered, { gap: Spacing.large, flexGrow: 1 } ]}>
            <View style={{ gap: Spacing.small, alignItems: 'center' }}>
                <Text style={[ CommonStyles.title, { color: colors.text } ]}>{props.title}</Text>
                {props.description
                ? <Text style={[ CommonStyles.instruction, { color: colors.mutedText } ]}>{props.description}</Text>
                : null}
                {props.errorMessage
                ? <Text testID="pin-error" style={[ CommonStyles.instruction, { color: colors.danger } ]}>{props.errorMessage}</Text>
                : null}
            </View>
            <PinPad
                value={pin}
                onChange={setPin}
                disabled={isSubmitting}
                extraKeyIcon={props.extraKeyIcon}
                extraKeyLabel={props.extraKeyLabel}
                onExtraKeyPressed={props.onExtraKeyPressed}
            />
            <AppButton
                title={props.submitTitle ?? t('Pin_Confirm')}
                testID="pin-submit"
                disabled={pin.length < MinimumPinLength}
                isLoading={isSubmitting}
                onPress={submit}
            />
            {pin.length > 0 && pin.length < MinimumPinLength
            ? <Text style={[ CommonStyles.instruction, { color: colors.mutedText } ]}>{t('Pin_MinimumLength', { count: MinimumPinLength })}</Text>
            : null}
            {props.footer}
        </ScrollView>
    );
};
