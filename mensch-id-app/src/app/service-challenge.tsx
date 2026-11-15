import { useEffect, useState } from 'react';
import { ActivityIndicator, ScrollView, Text } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/AuthContext';
import { ApiError } from '../communication/ApiClient';
import { AppButton } from '../components/AppButton';
import { ServiceChallengeView } from '../components/ServiceChallengeView';
import { isValidServiceChallengeId } from '../helpers/ServiceChallengeHelpers';
import { useThemeColors } from '../hooks/useThemeColors';
import { CommonStyles, Spacing } from '../styles/Theme';
import { ServiceChallengeAnswer } from '../types/frontendTypes';
import { ViewModels } from '../types/viewModels';

const ServiceChallengeScreen = () => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const router = useRouter();
    const auth = useAuth();
    const { id } = useLocalSearchParams<{ id: string }>();
    const [ challenge, setChallenge ] = useState<ViewModels.ServiceChallengeViewModel>();
    const isIdValid = isValidServiceChallengeId(id);
    const [ isLoading, setIsLoading ] = useState<boolean>(isIdValid);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ answer, setAnswer ] = useState<ServiceChallengeAnswer>();
    const [ errorMessage, setErrorMessage ] = useState<string>();

    useEffect(() => {
        if(!isIdValid) {
            return;
        }
        let isCancelled = false;
        const loadChallenge = async () => {
            try {
                const loadedChallenge = await auth.apiClient.get<ViewModels.ServiceChallengeViewModel>(`api/service-challenges/${id}`);
                if(!isCancelled) {
                    setChallenge(loadedChallenge);
                }
            } catch (error) {
                const isNotFound = error instanceof ApiError && error.statusCode === 404;
                if(!isCancelled) {
                    setErrorMessage(t(isNotFound ? 'Challenge_NotFound' : 'Challenge_CouldNotLoad'));
                }
            } finally {
                if(!isCancelled) {
                    setIsLoading(false);
                }
            }
        };
        loadChallenge();
        return () => {
            isCancelled = true;
        };
    }, [ id, isIdValid, auth.apiClient, t ]);

    const submitAnswer = async (selectedAnswer: ServiceChallengeAnswer) => {
        setIsSubmitting(true);
        setErrorMessage(undefined);
        try {
            await auth.apiClient.postWithoutResponse(`api/service-challenges/${id}/${selectedAnswer}`);
            setAnswer(selectedAnswer);
        } catch (error) {
            const isConflictOrGone = error instanceof ApiError && (error.statusCode === 409 || error.statusCode === 410);
            setErrorMessage(t(isConflictOrGone ? 'ServiceChallenge_NoLongerAnswerable' : 'ServiceChallenge_CouldNotAnswer'));
        } finally {
            setIsSubmitting(false);
        }
    };

    if(isLoading) {
        return (
            <ScrollView contentContainerStyle={[ CommonStyles.screen, CommonStyles.centered ]}>
                <ActivityIndicator size="large" color={colors.primary} />
            </ScrollView>
        );
    }
    if(answer) {
        return (
            <ScrollView contentContainerStyle={[ CommonStyles.screen, CommonStyles.centered, { gap: Spacing.large } ]}>
                <Text testID="service-challenge-result" style={[ CommonStyles.title, { color: colors.text } ]}>
                    {t(answer === 'accept' ? 'ServiceChallenge_Accepted' : 'ServiceChallenge_Rejected')}
                </Text>
                <AppButton title={t('ServiceChallenge_Done')} onPress={() => router.canDismiss() ? router.dismissTo('/') : router.replace('/')} />
            </ScrollView>
        );
    }
    return (
        <ScrollView contentContainerStyle={[ CommonStyles.screen, { gap: Spacing.large } ]}>
            {challenge
            ? <ServiceChallengeView
                challenge={challenge}
                isSubmitting={isSubmitting}
                onAccept={() => submitAnswer('accept')}
                onReject={() => submitAnswer('reject')}
            />
            : null}
            {!isIdValid
            ? <Text style={[ CommonStyles.instruction, { color: colors.danger } ]}>{t('Challenge_NotFound')}</Text>
            : null}
            {errorMessage
            ? <Text style={[ CommonStyles.instruction, { color: colors.danger } ]}>{errorMessage}</Text>
            : null}
        </ScrollView>
    );
};

export default ServiceChallengeScreen;
