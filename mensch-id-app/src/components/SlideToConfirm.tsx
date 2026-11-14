import { useState } from 'react';
import { LayoutChangeEvent, Text, View } from 'react-native';
import { GestureDetector, usePanGesture } from 'react-native-gesture-handler';
import Animated, { useAnimatedStyle, useSharedValue, withSpring, withTiming } from 'react-native-reanimated';
import { scheduleOnRN } from 'react-native-worklets';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { useThemeColors } from '../hooks/useThemeColors';

const KnobSize = 56;
const ConfirmThreshold = 0.9;

interface SlideToConfirmProps {
    title: string;
    onConfirm: () => void;
    disabled?: boolean;
    testID?: string;
}

export const SlideToConfirm = (props: SlideToConfirmProps) => {

    const colors = useThemeColors();
    const [ trackWidth, setTrackWidth ] = useState<number>(0);
    const translation = useSharedValue<number>(0);
    const maximumTranslation = Math.max(0, trackWidth - KnobSize);
    const isDisabled = !!props.disabled;

    const panGesture = usePanGesture({
        enabled: !isDisabled,
        onUpdate: event => {
            'worklet';
            translation.value = Math.min(Math.max(event.translationX, 0), maximumTranslation);
        },
        onDeactivate: () => {
            'worklet';
            const isConfirmed = maximumTranslation > 0 && translation.value >= maximumTranslation * ConfirmThreshold;
            if(isConfirmed) {
                translation.value = withTiming(maximumTranslation);
                scheduleOnRN(props.onConfirm);
                return;
            }
            translation.value = withSpring(0);
        }
    });

    const knobStyle = useAnimatedStyle(() => ({
        transform: [ { translateX: translation.value } ]
    }));

    const fillStyle = useAnimatedStyle(() => ({
        width: translation.value + KnobSize
    }));

    const onTrackLayout = (event: LayoutChangeEvent) => {
        setTrackWidth(event.nativeEvent.layout.width);
    };

    return (
        <View
            testID={props.testID}
            accessibilityRole="adjustable"
            accessibilityLabel={props.title}
            accessibilityState={{ disabled: isDisabled }}
            accessibilityActions={[ { name: 'activate' } ]}
            onAccessibilityAction={() => !isDisabled && props.onConfirm()}
            onLayout={onTrackLayout}
            style={{
                height: KnobSize,
                borderRadius: KnobSize / 2,
                backgroundColor: colors.surface,
                borderColor: colors.border,
                borderWidth: 1,
                justifyContent: 'center',
                overflow: 'hidden',
                opacity: isDisabled ? 0.5 : 1
            }}
        >
            <Animated.View
                style={[
                    { position: 'absolute', left: 0, top: 0, bottom: 0, borderRadius: KnobSize / 2, backgroundColor: colors.primary, opacity: 0.25 },
                    fillStyle
                ]}
            />
            <Text style={{ textAlign: 'center', fontSize: 16, fontWeight: '600', color: colors.text, paddingLeft: KnobSize }}>
                {props.title}
            </Text>
            <GestureDetector gesture={panGesture}>
                <Animated.View
                    style={[
                        {
                            position: 'absolute',
                            left: 0,
                            width: KnobSize,
                            height: KnobSize,
                            borderRadius: KnobSize / 2,
                            backgroundColor: colors.primary,
                            alignItems: 'center',
                            justifyContent: 'center'
                        },
                        knobStyle
                    ]}
                >
                    <MaterialCommunityIcons name="chevron-double-right" size={28} color={colors.onPrimary} />
                </Animated.View>
            </GestureDetector>
        </View>
    );
};
