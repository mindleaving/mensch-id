import { useState } from 'react';
import { Pressable, Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { AppButton } from './AppButton';
import { ChallengeSecretView } from './ChallengeSecretView';
import { CommonStyles, Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';
import { Models } from '../types/models';

interface ChallengeListItemProps {
    challenge: Models.MenschIdChallenge;
    isExpanded: boolean;
    onToggle: () => void;
}

export const ChallengeListItem = (props: ChallengeListItemProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const [ isSecretVisible, setIsSecretVisible ] = useState<boolean>(false);

    const toggle = () => {
        setIsSecretVisible(false);
        props.onToggle();
    };

    return (
        <View style={{ borderWidth: 1, borderColor: colors.border, borderRadius: 8, backgroundColor: colors.surface }}>
            <Pressable
                accessibilityRole="button"
                accessibilityState={{ expanded: props.isExpanded }}
                testID={`challenge-${props.challenge.id}`}
                onPress={toggle}
                style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'center', padding: Spacing.medium, gap: Spacing.small }}
            >
                <Text style={[ CommonStyles.largeValue, { color: colors.text, fontSize: 24 } ]}>
                    {props.challenge.challengeShortId}
                </Text>
                <MaterialCommunityIcons
                    name={props.isExpanded ? 'chevron-up' : 'chevron-down'}
                    size={24}
                    color={colors.mutedText}
                />
            </Pressable>
            {props.isExpanded
            ? <View style={{ padding: Spacing.medium, paddingTop: 0, gap: Spacing.medium }}>
                {isSecretVisible
                ? <ChallengeSecretView challenge={props.challenge} />
                : <>
                    <View style={[ CommonStyles.notice, { backgroundColor: colors.warningBackground } ]}>
                        <Text style={[ CommonStyles.instruction, { color: colors.warning } ]}>
                            {t('ReceiveChallenge_Instruction')}
                        </Text>
                        <Text style={[ CommonStyles.instruction, { color: colors.warning, marginTop: Spacing.small } ]}>
                            {t('ReceiveChallenge_Secret_Description')}
                        </Text>
                    </View>
                    <AppButton
                        title={t('ReceiveChallenge_Secret_Show')}
                        icon="eye"
                        variant="danger"
                        testID="show-secret"
                        onPress={() => setIsSecretVisible(true)}
                    />
                </>}
            </View>
            : null}
        </View>
    );
};
