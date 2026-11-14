import { useEffect, useState } from 'react';
import { Pressable, Text, View } from 'react-native';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { getActiveServiceChallenges, getFirstLine } from '../helpers/ServiceChallengeHelpers';
import { useThemeColors } from '../hooks/useThemeColors';
import { CommonStyles, Spacing } from '../styles/Theme';
import { ViewModels } from '../types/viewModels';

const ExpiryCheckIntervalMilliseconds = 5000;

interface PendingServiceChallengesProps {
    challenges: ViewModels.ServiceChallengeViewModel[];
    onChallengeSelected: (challengeId: string) => void;
}

export const PendingServiceChallenges = (props: PendingServiceChallengesProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const [ now, setNow ] = useState<number>(() => Date.now());
    const activeChallenges = getActiveServiceChallenges(props.challenges, now);

    useEffect(() => {
        const interval = setInterval(() => setNow(Date.now()), ExpiryCheckIntervalMilliseconds);
        return () => clearInterval(interval);
    }, []);

    if(activeChallenges.length === 0) {
        return null;
    }
    return (
        <View testID="pending-service-challenges" style={{ gap: Spacing.small }}>
            <Text style={[ CommonStyles.label, { color: colors.text } ]}>{t('ServiceChallenge_PendingTitle')}</Text>
            {activeChallenges.map(challenge => (
                <Pressable
                    key={challenge.id}
                    accessibilityRole="button"
                    onPress={() => props.onChallengeSelected(challenge.id)}
                    style={({ pressed }) => ({
                        flexDirection: 'row',
                        alignItems: 'center',
                        gap: Spacing.small,
                        padding: Spacing.medium,
                        borderRadius: 8,
                        backgroundColor: colors.warningBackground,
                        opacity: pressed ? 0.8 : 1
                    })}
                >
                    <MaterialCommunityIcons name="shield-account" size={24} color={colors.warning} />
                    <View style={{ flex: 1 }}>
                        <Text style={{ color: colors.text, fontWeight: '600' }}>{challenge.serviceDisplayName}</Text>
                        <Text numberOfLines={1} style={{ color: colors.mutedText }}>{getFirstLine(challenge.description)}</Text>
                    </View>
                    <MaterialCommunityIcons name="chevron-right" size={24} color={colors.mutedText} />
                </Pressable>
            ))}
        </View>
    );
};
