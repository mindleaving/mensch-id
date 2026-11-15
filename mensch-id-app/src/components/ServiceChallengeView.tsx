import { useEffect, useState } from 'react';
import { Pressable, Text, View } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { AppButton } from './AppButton';
import { SlideToConfirm } from './SlideToConfirm';
import { formatRemainingTime, getRemainingSeconds } from '../helpers/ServiceChallengeHelpers';
import { useThemeColors } from '../hooks/useThemeColors';
import { CommonStyles, Spacing } from '../styles/Theme';
import { ViewModels } from '../types/viewModels';

interface ServiceChallengeViewProps {
    challenge: ViewModels.ServiceChallengeViewModel;
    isSubmitting: boolean;
    onAccept: () => void;
    onReject: () => void;
}

export const ServiceChallengeView = (props: ServiceChallengeViewProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const [ isContactExpanded, setIsContactExpanded ] = useState<boolean>(false);
    const [ remainingSeconds, setRemainingSeconds ] = useState<number>(() => getRemainingSeconds(props.challenge.expiresAt, Date.now()));
    const isExpired = remainingSeconds <= 0;

    useEffect(() => {
        const interval = setInterval(() => {
            setRemainingSeconds(getRemainingSeconds(props.challenge.expiresAt, Date.now()));
        }, 1000);
        return () => clearInterval(interval);
    }, [ props.challenge.expiresAt ]);

    return (
        <View style={{ gap: Spacing.large }}>
            <View style={{ gap: Spacing.small }}>
                <Text style={[ CommonStyles.title, { color: colors.text } ]}>{props.challenge.serviceDisplayName}</Text>
                <Pressable
                    testID="service-contact-toggle"
                    accessibilityRole="button"
                    accessibilityState={{ expanded: isContactExpanded }}
                    onPress={() => setIsContactExpanded(!isContactExpanded)}
                    style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: Spacing.small }}
                >
                    <Text style={{ color: colors.primary }}>{t('ServiceChallenge_ContactInformation')}</Text>
                    <MaterialCommunityIcons name={isContactExpanded ? 'chevron-up' : 'chevron-down'} size={20} color={colors.primary} />
                </Pressable>
                {isContactExpanded
                ? <Text testID="service-contact" selectable style={{ color: colors.mutedText, textAlign: 'center' }}>
                    {props.challenge.serviceContactInformation}
                </Text>
                : null}
            </View>
            <View style={{ backgroundColor: colors.surface, borderRadius: 8, padding: Spacing.medium, gap: Spacing.small }}>
                <Text style={[ CommonStyles.label, { color: colors.text, textAlign: 'left' } ]}>{t('ServiceChallenge_Request')}</Text>
                <Text testID="service-challenge-description" style={{ color: colors.text, fontSize: 16 }}>{props.challenge.description}</Text>
            </View>
            <Text style={[ CommonStyles.instruction, { color: isExpired ? colors.danger : colors.mutedText } ]}>
                {isExpired
                ? t('ServiceChallenge_Expired')
                : t('ServiceChallenge_ExpiresIn', { time: formatRemainingTime(remainingSeconds) })}
            </Text>
            <SlideToConfirm
                testID="service-challenge-accept"
                title={t('ServiceChallenge_SlideToAccept')}
                disabled={isExpired || props.isSubmitting}
                onConfirm={props.onAccept}
            />
            <View style={{ height: Spacing.large }} />
            <AppButton
                testID="service-challenge-reject"
                title={t('ServiceChallenge_Reject')}
                icon="close"
                variant="danger"
                disabled={isExpired || props.isSubmitting}
                onPress={props.onReject}
            />
        </View>
    );
};
