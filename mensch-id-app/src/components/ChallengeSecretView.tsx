import { Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { CommonStyles, Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';
import { Models } from '../types/models';

interface ChallengeSecretViewProps {
    challenge: Models.MenschIdChallenge;
}

export const ChallengeSecretView = (props: ChallengeSecretViewProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();

    return (
        <View style={{ gap: Spacing.medium, alignItems: 'center' }}>
            <Text style={[ CommonStyles.instruction, { color: colors.text } ]}>
                {t('ReceiveChallenge_Secret_Description')}
            </Text>
            <View style={{ alignItems: 'center' }}>
                <Text style={[ CommonStyles.label, { color: colors.mutedText } ]}>{t('Challenge_ID')}</Text>
                <Text style={[ CommonStyles.largeValue, { color: colors.text } ]}>{props.challenge.challengeShortId}</Text>
            </View>
            <View style={{ alignItems: 'center' }}>
                <Text style={[ CommonStyles.label, { color: colors.mutedText } ]}>{t('Challenge_Secret')}</Text>
                <Text
                    testID="challenge-secret"
                    selectable
                    style={[ CommonStyles.largeValue, { color: colors.primary } ]}
                >
                    {props.challenge.challengeSecret}
                </Text>
            </View>
        </View>
    );
};
