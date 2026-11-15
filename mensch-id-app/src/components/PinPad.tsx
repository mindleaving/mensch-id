import { ComponentProps } from 'react';
import { Pressable, Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';
import { MaximumPinLength } from '../lock/AppLockService';

interface PinPadProps {
    value: string;
    onChange: (value: string) => void;
    disabled?: boolean;
    extraKeyIcon?: ComponentProps<typeof MaterialCommunityIcons>['name'];
    extraKeyLabel?: string;
    onExtraKeyPressed?: () => void;
}

export const PinPad = (props: PinPadProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const digitRows = [ [ '1', '2', '3' ], [ '4', '5', '6' ], [ '7', '8', '9' ] ];

    const appendDigit = (digit: string) => {
        if(props.value.length >= MaximumPinLength) {
            return;
        }
        props.onChange(props.value + digit);
    };

    const removeDigit = () => {
        props.onChange(props.value.slice(0, -1));
    };

    const keyStyle = {
        width: 76,
        height: 76,
        borderRadius: 38,
        alignItems: 'center' as const,
        justifyContent: 'center' as const,
        backgroundColor: colors.surface
    };

    const renderDigitKey = (digit: string) => (
        <Pressable
            key={digit}
            accessibilityRole="button"
            accessibilityLabel={digit}
            testID={`pin-key-${digit}`}
            disabled={props.disabled}
            onPress={() => appendDigit(digit)}
            style={({ pressed }) => [ keyStyle, { opacity: pressed ? 0.6 : 1 } ]}
        >
            <Text style={{ fontSize: 28, fontWeight: '500', color: colors.text }}>{digit}</Text>
        </Pressable>
    );

    return (
        <View style={{ alignItems: 'center', gap: Spacing.large }}>
            <View
                testID="pin-dots"
                accessibilityLabel={t('Pin_DigitsEntered', { count: props.value.length })}
                style={{ flexDirection: 'row', gap: Spacing.small, minHeight: 16 }}
            >
                {Array.from({ length: props.value.length }, (_, index) => (
                    <View key={index} style={{ width: 14, height: 14, borderRadius: 7, backgroundColor: colors.text }} />
                ))}
            </View>
            <View style={{ gap: Spacing.medium }}>
                {digitRows.map(row => (
                    <View key={row.join('')} style={{ flexDirection: 'row', gap: Spacing.large }}>
                        {row.map(renderDigitKey)}
                    </View>
                ))}
                <View style={{ flexDirection: 'row', gap: Spacing.large }}>
                    {props.extraKeyIcon && props.onExtraKeyPressed
                    ? <Pressable
                        accessibilityRole="button"
                        accessibilityLabel={props.extraKeyLabel}
                        testID="pin-key-extra"
                        disabled={props.disabled}
                        onPress={props.onExtraKeyPressed}
                        style={({ pressed }) => [ keyStyle, { backgroundColor: 'transparent', opacity: pressed ? 0.6 : 1 } ]}
                    >
                        <MaterialCommunityIcons name={props.extraKeyIcon} size={32} color={colors.primary} />
                    </Pressable>
                    : <View style={[ keyStyle, { backgroundColor: 'transparent' } ]} />}
                    {renderDigitKey('0')}
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel={t('Pin_Delete')}
                        testID="pin-key-delete"
                        disabled={props.disabled || props.value.length === 0}
                        onPress={removeDigit}
                        style={({ pressed }) => [ keyStyle, { backgroundColor: 'transparent', opacity: pressed ? 0.6 : 1 } ]}
                    >
                        <MaterialCommunityIcons name="backspace-outline" size={28} color={colors.text} />
                    </Pressable>
                </View>
            </View>
        </View>
    );
};
