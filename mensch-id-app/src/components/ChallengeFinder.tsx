import { useState } from 'react';
import { ActivityIndicator, Text, TextInput, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { ScanButton } from './ScanButton';
import { ChallengeListItem } from './ChallengeListItem';
import { CommonStyles, Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';
import { useChallengeSearch } from '../hooks/useChallengeSearch';
import { normalizeChallengeSearchText } from '../helpers/ChallengeHelpers';
import { ChallengeSearchMinimumLength } from '../helpers/ConfigurationHelpers';
import { Models } from '../types/models';

interface ChallengeFinderProps {
    searchChallenges: (searchText: string) => Promise<Models.MenschIdChallenge[]>;
    onScanPressed: () => void;
}

export const ChallengeFinder = (props: ChallengeFinderProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const [ searchText, setSearchText ] = useState<string>('');
    const [ expandedChallengeId, setExpandedChallengeId ] = useState<string>();
    const searchState = useChallengeSearch(searchText, props.searchChallenges);
    const isSearchActive = searchText.length >= ChallengeSearchMinimumLength;

    const toggleChallenge = (challengeId: string) => {
        setExpandedChallengeId(currentId => currentId === challengeId ? undefined : challengeId);
    };

    return (
        <View style={{ gap: Spacing.medium, alignItems: 'stretch' }}>
            <View style={{ alignItems: 'center' }}>
                <Text nativeID="challenge-label" style={[ CommonStyles.label, { color: colors.text } ]}>{t('Challenge')}</Text>
                <TextInput
                    testID="challenge-input"
                    accessibilityLabelledBy="challenge-label"
                    accessibilityLabel={t('Challenge')}
                    value={searchText}
                    onChangeText={text => setSearchText(normalizeChallengeSearchText(text))}
                    placeholder={t('Challenge_Placeholder')}
                    placeholderTextColor={colors.mutedText}
                    autoCapitalize="characters"
                    autoCorrect={false}
                    maxLength={64}
                    style={[ CommonStyles.input, { color: colors.text, borderColor: colors.border } ]}
                />
            </View>
            {!isSearchActive
            ? <View style={{ alignItems: 'center' }}>
                <ScanButton testID="scan-button" onPress={props.onScanPressed} />
            </View>
            : <View style={{ gap: Spacing.small }}>
                {searchState.isSearching && searchState.challenges.length === 0
                ? <ActivityIndicator color={colors.primary} />
                : null}
                {searchState.hasError
                ? <Text style={[ CommonStyles.instruction, { color: colors.danger } ]}>{t('Challenge_CouldNotSearch')}</Text>
                : null}
                {!searchState.isSearching && !searchState.hasError && searchState.challenges.length === 0
                ? <Text style={[ CommonStyles.instruction, { color: colors.mutedText } ]}>{t('Challenge_NoMatches')}</Text>
                : null}
                {searchState.challenges.map(challenge => (
                    <ChallengeListItem
                        key={challenge.id}
                        challenge={challenge}
                        isExpanded={expandedChallengeId === challenge.id}
                        onToggle={() => toggleChallenge(challenge.id)}
                    />
                ))}
            </View>}
        </View>
    );
};
