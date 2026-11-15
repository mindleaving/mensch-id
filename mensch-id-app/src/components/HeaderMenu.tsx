import { useState } from 'react';
import { Modal, Pressable, Text, View } from 'react-native';
import { useTranslation } from 'react-i18next';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Spacing } from '../styles/Theme';
import { useThemeColors } from '../hooks/useThemeColors';

interface HeaderMenuProps {
    onCreateChallenge: () => void;
    onOpenSettings: () => void;
    onLogout: () => void;
}

export const HeaderMenu = (props: HeaderMenuProps) => {

    const { t } = useTranslation();
    const colors = useThemeColors();
    const insets = useSafeAreaInsets();
    const [ isOpen, setIsOpen ] = useState<boolean>(false);

    const select = (action: () => void) => {
        setIsOpen(false);
        action();
    };

    return (
        <>
            <Pressable
                accessibilityRole="button"
                accessibilityLabel={t('Menu')}
                testID="menu-button"
                hitSlop={12}
                onPress={() => setIsOpen(true)}
            >
                <MaterialCommunityIcons name="dots-vertical" size={26} color={colors.text} />
            </Pressable>
            <Modal
                visible={isOpen}
                transparent
                animationType="fade"
                onRequestClose={() => setIsOpen(false)}
            >
                <Pressable style={{ flex: 1 }} onPress={() => setIsOpen(false)}>
                    <View
                        style={{
                            position: 'absolute',
                            top: insets.top + 48,
                            right: Spacing.medium,
                            minWidth: 220,
                            borderRadius: 8,
                            backgroundColor: colors.surface,
                            borderColor: colors.border,
                            borderWidth: 1,
                            elevation: 6,
                            shadowColor: '#000',
                            shadowOpacity: 0.2,
                            shadowRadius: 8
                        }}
                    >
                        <Pressable
                            accessibilityRole="menuitem"
                            testID="menu-create-challenge"
                            onPress={() => select(props.onCreateChallenge)}
                            style={{ flexDirection: 'row', alignItems: 'center', gap: Spacing.small, padding: Spacing.medium }}
                        >
                            <MaterialCommunityIcons name="plus-circle-outline" size={22} color={colors.text} />
                            <Text style={{ fontSize: 16, color: colors.text }}>{t('Menu_CreateChallenge')}</Text>
                        </Pressable>
                        <Pressable
                            accessibilityRole="menuitem"
                            testID="menu-settings"
                            onPress={() => select(props.onOpenSettings)}
                            style={{ flexDirection: 'row', alignItems: 'center', gap: Spacing.small, padding: Spacing.medium }}
                        >
                            <MaterialCommunityIcons name="cog-outline" size={22} color={colors.text} />
                            <Text style={{ fontSize: 16, color: colors.text }}>{t('Menu_Settings')}</Text>
                        </Pressable>
                        <Pressable
                            accessibilityRole="menuitem"
                            testID="menu-logout"
                            onPress={() => select(props.onLogout)}
                            style={{ flexDirection: 'row', alignItems: 'center', gap: Spacing.small, padding: Spacing.medium }}
                        >
                            <MaterialCommunityIcons name="logout" size={22} color={colors.danger} />
                            <Text style={{ fontSize: 16, color: colors.danger, fontWeight: '600' }}>{t('Menu_Logout')}</Text>
                        </Pressable>
                    </View>
                </Pressable>
            </Modal>
        </>
    );
};
