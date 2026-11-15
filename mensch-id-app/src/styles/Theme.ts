import { StyleSheet } from 'react-native';

export const LightColors = {
    background: '#ffffff',
    surface: '#f2f3f5',
    text: '#1e1e1e',
    mutedText: '#5f6368',
    primary: '#0d6efd',
    onPrimary: '#ffffff',
    danger: '#dc3545',
    warning: '#8a5300',
    warningBackground: '#fff3cd',
    border: '#d0d4d9'
};

export const DarkColors: typeof LightColors = {
    background: '#121212',
    surface: '#1e1f22',
    text: '#f1f1f1',
    mutedText: '#a8adb3',
    primary: '#4c8dff',
    onPrimary: '#ffffff',
    danger: '#ff6b6b',
    warning: '#ffd27a',
    warningBackground: '#3a2f12',
    border: '#3a3d42'
};

export type ThemeColors = typeof LightColors;

export const Spacing = {
    small: 8,
    medium: 16,
    large: 24
};

export const CommonStyles = StyleSheet.create({
    screen: {
        flex: 1,
        padding: Spacing.medium
    },
    centered: {
        alignItems: 'center',
        justifyContent: 'center'
    },
    title: {
        fontSize: 22,
        fontWeight: '600',
        textAlign: 'center'
    },
    menschId: {
        fontSize: 28,
        fontWeight: '700',
        letterSpacing: 1,
        textAlign: 'center'
    },
    label: {
        fontSize: 16,
        fontWeight: '600',
        textAlign: 'center',
        marginBottom: Spacing.small
    },
    input: {
        fontSize: 22,
        letterSpacing: 2,
        textAlign: 'center',
        borderWidth: 1,
        borderRadius: 8,
        paddingVertical: 10,
        paddingHorizontal: Spacing.medium,
        minWidth: 240
    },
    button: {
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        gap: Spacing.small,
        borderRadius: 8,
        paddingVertical: 12,
        paddingHorizontal: Spacing.large
    },
    buttonText: {
        fontSize: 16,
        fontWeight: '600'
    },
    largeValue: {
        fontSize: 34,
        fontWeight: '700',
        letterSpacing: 3,
        textAlign: 'center'
    },
    instruction: {
        fontSize: 15,
        textAlign: 'center',
        lineHeight: 21
    },
    notice: {
        borderRadius: 8,
        padding: Spacing.medium
    }
});
