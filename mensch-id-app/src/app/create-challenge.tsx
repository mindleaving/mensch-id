import { useState } from 'react';
import { KeyboardAvoidingView, Platform, ScrollView, Text, TextInput, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/AuthContext';
import { AppButton } from '../components/AppButton';
import { CreatedChallengeView } from '../components/CreatedChallengeView';
import { DefaultChallengeLength } from '../helpers/ConfigurationHelpers';
import { isMenschId, normalizeMenschId } from '../helpers/MenschIdHelpers';
import { useThemeColors } from '../hooks/useThemeColors';
import { CommonStyles, Spacing } from '../styles/Theme';
import { Models } from '../types/models';

const CreateChallengeScreen = () => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const auth = useAuth();
    const [ menschId, setMenschId ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ hasError, setHasError ] = useState<boolean>(false);
    const [ challenge, setChallenge ] = useState<Models.MenschIdChallenge>();
    const isValidMenschId = isMenschId(menschId);

    const createChallenge = async () => {
        if(!isValidMenschId) {
            return;
        }
        setIsSubmitting(true);
        setHasError(false);
        try {
            const encodedMenschId = encodeURIComponent(normalizeMenschId(menschId));
            const createdChallenge = await auth.apiClient.post<Models.MenschIdChallenge>(
                `api/id/${encodedMenschId}/challenge?challengeLength=${DefaultChallengeLength}`);
            setChallenge(createdChallenge);
        } catch {
            setHasError(true);
        } finally {
            setIsSubmitting(false);
        }
    };

    const startOver = () => {
        setChallenge(undefined);
        setMenschId('');
    };

    if(challenge) {
        return (
            <ScrollView contentContainerStyle={[ CommonStyles.screen, CommonStyles.centered, { gap: Spacing.large } ]}>
                <CreatedChallengeView challenge={challenge} />
                <AppButton title={t('NewChallenge')} icon="refresh" variant="outline" onPress={startOver} />
            </ScrollView>
        );
    }
    return (
        <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
            <ScrollView
                keyboardShouldPersistTaps="handled"
                contentContainerStyle={[ CommonStyles.screen, { gap: Spacing.large, justifyContent: 'center', flexGrow: 1 } ]}
            >
                <Text style={[ CommonStyles.instruction, { color: colors.text } ]}>{t('SendChallenge_Text')}</Text>
                <View style={{ alignItems: 'center' }}>
                    <Text nativeID="mensch-id-label" style={[ CommonStyles.label, { color: colors.text } ]}>{t('CreateChallenge_MenschId')}</Text>
                    <TextInput
                        testID="target-mensch-id"
                        accessibilityLabelledBy="mensch-id-label"
                        accessibilityLabel={t('CreateChallenge_MenschId')}
                        value={menschId}
                        onChangeText={text => setMenschId(text.toUpperCase())}
                        placeholder={t('MenschIDFormatPlaceholder')}
                        placeholderTextColor={colors.mutedText}
                        autoCapitalize="characters"
                        autoCorrect={false}
                        maxLength={14}
                        onSubmitEditing={createChallenge}
                        style={[ CommonStyles.input, { color: colors.text, borderColor: colors.border } ]}
                    />
                    {menschId.length >= 14 && !isValidMenschId
                    ? <Text style={[ CommonStyles.instruction, { color: colors.danger, marginTop: Spacing.small } ]}>{t('CreateChallenge_InvalidMenschId')}</Text>
                    : null}
                </View>
                <View style={{ alignItems: 'center' }}>
                    <AppButton
                        title={t('CreateChallenge_Submit')}
                        icon="shield-check"
                        testID="create-challenge-submit"
                        disabled={!isValidMenschId}
                        isLoading={isSubmitting}
                        onPress={createChallenge}
                    />
                </View>
                {hasError
                ? <Text style={[ CommonStyles.instruction, { color: colors.danger } ]}>{t('Challenge_CouldNotSend')}</Text>
                : null}
            </ScrollView>
        </KeyboardAvoidingView>
    );
};

export default CreateChallengeScreen;
