import i18next from 'i18next';
import { initReactI18next } from 'react-i18next';
import { getLocales } from 'expo-localization';
import en from './en.json';
import de from './de.json';

const SupportedLanguages = [ 'en', 'de' ];

const getDeviceLanguage = (): string => {
    const deviceLanguage = getLocales()[0]?.languageCode ?? 'en';
    return SupportedLanguages.includes(deviceLanguage) ? deviceLanguage : 'en';
};

i18next
    .use(initReactI18next)
    .init({
        resources: {
            en: { translation: en },
            de: { translation: de }
        },
        lng: getDeviceLanguage(),
        fallbackLng: 'en',
        interpolation: { escapeValue: false }
    });

export default i18next;
