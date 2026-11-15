import { useCallback } from 'react';
import { ActivityIndicator, KeyboardAvoidingView, Platform, ScrollView, Text, View } from 'react-native';
import { Stack, useRouter } from 'expo-router';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../auth/AuthContext';
import { ChallengeFinder } from '../components/ChallengeFinder';
import { HeaderMenu } from '../components/HeaderMenu';
import { useMyProfile } from '../hooks/useMyProfile';
import { useThemeColors } from '../hooks/useThemeColors';
import { CommonStyles, Spacing } from '../styles/Theme';
import { Models } from '../types/models';

const HomeScreen = () => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const router = useRouter();
    const auth = useAuth();
    const profile = useMyProfile();

    const searchChallenges = useCallback(async (searchText: string) => {
        const encodedSearchText = encodeURIComponent(searchText);
        return await auth.apiClient.get<Models.MenschIdChallenge[]>(`api/challenges?searchText=${encodedSearchText}&count=10`);
    }, [ auth.apiClient ]);

    return (
        <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
            <Stack.Screen
                options={{
                    headerRight: () => (
                        <HeaderMenu
                            onCreateChallenge={() => router.push('/create-challenge')}
                            onOpenSettings={() => router.push('/settings')}
                            onLogout={auth.logout}
                        />
                    )
                }}
            />
            <ScrollView
                keyboardShouldPersistTaps="handled"
                contentContainerStyle={[ CommonStyles.screen, { gap: Spacing.large } ]}
            >
                <View style={{ alignItems: 'center', minHeight: 48, justifyContent: 'center' }}>
                    {profile.status === 'loading' ? <ActivityIndicator color={colors.primary} /> : null}
                    {profile.status === 'loaded'
                    ? <Text testID="mensch-id" selectable style={[ CommonStyles.menschId, { color: colors.text } ]}>{profile.menschId}</Text>
                    : null}
                    {profile.status === 'noProfile'
                    ? <Text style={[ CommonStyles.instruction, { color: colors.mutedText } ]}>{t('Profile_NoMenschId')}</Text>
                    : null}
                    {profile.status === 'error'
                    ? <Text style={[ CommonStyles.instruction, { color: colors.danger } ]}>{t('Profile_CouldNotLoad')}</Text>
                    : null}
                </View>
                <ChallengeFinder
                    searchChallenges={searchChallenges}
                    onScanPressed={() => router.push('/scan')}
                />
            </ScrollView>
        </KeyboardAvoidingView>
    );
};

export default HomeScreen;
