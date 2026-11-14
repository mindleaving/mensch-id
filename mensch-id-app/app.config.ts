import { ExpoConfig } from 'expo/config';

const DefaultApiBaseUrl = 'https://mensch.id';
const AppLinkPathPrefix = '/app/auth';
const appLinkHost = new URL(process.env.EXPO_PUBLIC_API_URL ?? DefaultApiBaseUrl).host;

const config: ExpoConfig = {
    name: 'mensch.ID',
    slug: 'mensch-id',
    scheme: 'menschid',
    version: '1.0.0',
    orientation: 'portrait',
    icon: './assets/icon.png',
    userInterfaceStyle: 'automatic',
    ios: {
        bundleIdentifier: 'id.mensch.app',
        deploymentTarget: '17.4',
        supportsTablet: true,
        associatedDomains: [
            `applinks:${appLinkHost}`,
            `webcredentials:${appLinkHost}`
        ],
        infoPlist: {
            ITSAppUsesNonExemptEncryption: false
        }
    },
    android: {
        package: 'id.mensch.app',
        allowBackup: false,
        intentFilters: [
            {
                action: 'VIEW',
                autoVerify: true,
                data: [
                    {
                        scheme: 'https',
                        host: appLinkHost,
                        pathPrefix: AppLinkPathPrefix
                    }
                ],
                category: [ 'BROWSABLE', 'DEFAULT' ]
            }
        ],
        adaptiveIcon: {
            backgroundColor: '#1e1e1e',
            foregroundImage: './assets/android-icon-foreground.png',
            backgroundImage: './assets/android-icon-background.png',
            monochromeImage: './assets/android-icon-monochrome.png'
        },
        predictiveBackGestureEnabled: false
    },
    locales: {
        en: './locales/en.json',
        de: './locales/de.json'
    },
    plugins: [
        'expo-router',
        'expo-web-browser',
        'expo-secure-store',
        'expo-localization',
        [
            'expo-splash-screen',
            {
                image: './assets/splash-icon.png',
                imageWidth: 200,
                resizeMode: 'contain',
                backgroundColor: '#1e1e1e'
            }
        ],
        [
            'expo-local-authentication',
            {
                faceIDPermission: 'mensch.ID uses Face ID to unlock the app.'
            }
        ],
        [
            'expo-camera',
            {
                cameraPermission: 'mensch.ID uses the camera to scan challenge QR codes.',
                recordAudioAndroid: false
            }
        ]
    ],
    experiments: {
        typedRoutes: true
    }
};

export default config;
