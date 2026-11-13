import { useColorScheme } from 'react-native';
import { DarkColors, LightColors, ThemeColors } from '../styles/Theme';

export const useThemeColors = (): ThemeColors => {
    const colorScheme = useColorScheme();
    return colorScheme === 'dark' ? DarkColors : LightColors;
};
