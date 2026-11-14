import { useState } from 'react';
import { Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import QRCode from 'react-native-qrcode-svg';
import { AppButton } from './AppButton';
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
    const [ isIdHandedOver, setIsIdHandedOver ] = useState<boolean>(false);
    const [ isSecretVisible, setIsSecretVisible ] = useState<boolean>(false);

    if(!isIdHandedOver) {
        return (
            <View
                testID="challenge-step-id"
                style={[ CommonStyles.notice, { backgroundColor: colors.surface, gap: Spacing.large, alignItems: 'center', alignSelf: 'stretch' } ]}
            >
                <View style={{ alignItems: 'center' }}>
                    <Text style={[ CommonStyles.label, { color: colors.mutedText } ]}>{t('Challenge_ID')}</Text>
                    <Text testID="created-challenge-id" selectable style={[ CommonStyles.largeValue, { color: colors.text } ]}>
                        {props.challenge.challengeShortId}
                    </Text>
                </View>
                <Text style={[ CommonStyles.instruction, { color: colors.text } ]}>{t('CreateChallenge_IdInstruction')}</Text>
                <AppButton
                    title={t('CreateChallenge_IdDone')}
                    icon="arrow-right"
                    testID="challenge-id-done"
                    onPress={() => setIsIdHandedOver(true)}
                />
                <View
                    testID="challenge-qr-section"
                    style={{
                        alignSelf: 'stretch',
                        alignItems: 'center',
                        gap: Spacing.medium,
                        marginTop: Spacing.large,
                        paddingTop: Spacing.large,
                        borderTopWidth: 1,
                        borderTopColor: colors.border
                    }}
                >
                    <Text style={[ CommonStyles.instruction, { color: colors.mutedText } ]}>{t('CreateChallenge_QrCodeHint')}</Text>
                    <View style={{ padding: Spacing.medium, backgroundColor: '#ffffff', borderRadius: 8 }}>
                        <QRCode value={buildChallengeQrCodeContent(props.challenge.id)} size={180} />
                    </View>
                </View>
            </View>
        );
    }
    return (
        <View
            testID="challenge-step-secret"
            style={[ CommonStyles.notice, { backgroundColor: colors.surface, gap: Spacing.large, alignItems: 'center', alignSelf: 'stretch' } ]}
        >
            <Text testID="created-challenge-id-small" style={{ fontSize: 16, color: colors.mutedText, textAlign: 'center' }}>
                {t('Challenge_ID')}: <Text style={{ fontWeight: '700', color: colors.text }}>{props.challenge.challengeShortId}</Text>
            </Text>
            <View style={{ gap: Spacing.small }}>
                <Text style={[ CommonStyles.instruction, { color: colors.text } ]}>{t('CreateChallenge_SecretInstruction')}</Text>
                <Text style={[ CommonStyles.instruction, { color: colors.danger, fontWeight: '700' } ]}>
                    {t('SendChallenge_Secret_KeepSecret')}
                </Text>
            </View>
            {isSecretVisible
            ? <View style={{ alignItems: 'center' }}>
                <Text style={[ CommonStyles.label, { color: colors.mutedText } ]}>{t('Challenge_Secret')}</Text>
                <Text testID="created-challenge-secret" style={[ CommonStyles.largeValue, { color: colors.danger } ]}>
                    {props.challenge.challengeSecret}
                </Text>
            </View>
            : <AppButton
                title={t('CreateChallenge_ShowSecret')}
                icon="eye"
                variant="danger"
                testID="show-created-secret"
                onPress={() => setIsSecretVisible(true)}
            />}
        </View>
    );
};
