import { Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import QRCode from 'react-native-qrcode-svg';
import { CommonStyles, Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';
import { buildChallengeQrCodeContent } from '../helpers/ChallengeHelpers';
import { Models } from '../types/models';

interface CreatedChallengeViewProps {
    challenge: Models.MenschIdChallenge;
}

export const CreatedChallengeView = (props: CreatedChallengeViewProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();

    return (
        <View style={{ gap: Spacing.large, alignItems: 'center' }}>
            <Text style={[ CommonStyles.instruction, { color: colors.text } ]}>
                {t('CreateChallenge_ResultInstruction')}
            </Text>
            <View style={{ alignItems: 'center' }}>
                <Text style={[ CommonStyles.label, { color: colors.mutedText } ]}>{t('Challenge_ID')}</Text>
                <Text testID="created-challenge-id" selectable style={[ CommonStyles.largeValue, { color: colors.text } ]}>
                    {props.challenge.challengeShortId}
                </Text>
                <Text style={[ CommonStyles.instruction, { color: colors.mutedText } ]}>{t('SendChallenge_ID_Description')}</Text>
            </View>
            <View style={{ alignItems: 'center' }}>
                <Text style={[ CommonStyles.label, { color: colors.mutedText } ]}>{t('Challenge_Secret')}</Text>
                <Text testID="created-challenge-secret" style={[ CommonStyles.largeValue, { color: colors.primary } ]}>
                    {props.challenge.challengeSecret}
                </Text>
                <Text style={[ CommonStyles.instruction, { color: colors.danger, fontWeight: '700' } ]}>
                    {t('SendChallenge_Secret_KeepSecret')}
                </Text>
                <Text style={[ CommonStyles.instruction, { color: colors.mutedText } ]}>{t('SendChallenge_Secret_Description')}</Text>
            </View>
            <View style={{ padding: Spacing.medium, backgroundColor: '#ffffff', borderRadius: 8 }}>
                <QRCode value={buildChallengeQrCodeContent(props.challenge.id)} size={200} />
            </View>
        </View>
    );
};
