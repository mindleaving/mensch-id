import { toDictionary } from "./Transformations";

interface Translation {
    language: string;
    [key: string]: string;
}

export class Globalizer {
    preferredLanguage: string;
    defaultLanguage: string;
    translations: { [language:string] : Translation };

    constructor(
        preferredLanguages: readonly string[],
        defaultLanguage: string,
        translations: Translation[]) {
            const availableLanguages = translations.map(x => x.language.toLocaleLowerCase());
            this.defaultLanguage = defaultLanguage.toLocaleLowerCase();
            this.preferredLanguage = this.selectPreferredLanguage(
                preferredLanguages, 
                availableLanguages,
                this.defaultLanguage);
            this.translations = toDictionary(translations, x => x.language.toLocaleLowerCase());
    }

    selectPreferredLanguage = (
        preferredLanguages: readonly string[], 
        availableLanguages: string[], 
        defaultLanguage: string): string => {

        const matchingPreferredLanguage = preferredLanguages
            .map(language => language.toLocaleLowerCase())
            .find(language => availableLanguages.includes(language));
        return matchingPreferredLanguage ?? defaultLanguage;
    }

    canResolveText = (resourceId: string) => {
        const preferedTranslation = this.tryGetResourceDictionary(this.preferredLanguage);
        if(preferedTranslation && preferedTranslation[resourceId]) {
            return true;
        }
        const defaultTranslation = this.tryGetResourceDictionary(this.defaultLanguage);
        if(defaultTranslation && defaultTranslation[resourceId]) {
            return true;
        }
        return false;
    }

    resolveText = (resourceId: string, language?: string) => {
        if(language && language !== this.preferredLanguage) {
            const overrideTranslation = this.tryGetResourceDictionary(language);
            if(overrideTranslation && overrideTranslation[resourceId]) {
                return overrideTranslation[resourceId];
            }
        }
        const preferedTranslation = this.tryGetResourceDictionary(this.preferredLanguage);
        if(preferedTranslation && preferedTranslation[resourceId]) {
            return preferedTranslation[resourceId];
        }
        const defaultTranslation = this.tryGetResourceDictionary(this.defaultLanguage);
        if(defaultTranslation && defaultTranslation[resourceId]) {
            return defaultTranslation[resourceId];
        }
        throw new Error(`Cannot resolve resource '${resourceId}'`);
    }

    tryGetResourceDictionary = (language: string): { [key: string]: string } | undefined => {
        return this.translations[language];
    }
}
export const defaultGlobalizer: { instance?: Globalizer } = {}

export function setLanguage(languageId: string): void {
    defaultGlobalizer.instance!.preferredLanguage = languageId.toLocaleLowerCase();
}
export function getPreferredLanguage(): string {
    return defaultGlobalizer.instance!.preferredLanguage;
}
export function getFallbackLanguage(): string {
    return defaultGlobalizer.instance!.defaultLanguage;
}
export function canResolveText(resourceId: string): boolean {
    return defaultGlobalizer.instance!.canResolveText(resourceId);
}
export function resolveText(resourceId: string, language?: string): string {
    return defaultGlobalizer.instance!.resolveText(resourceId, language);
}
export function replaceResolvableText(str: string) {
    const resourceReferences = str.match(/resolveText:[a-zA-Z0-9_]+/g);
    if(!resourceReferences) {
        return str;
    }
    let output = str;
    for (const resourceReference of resourceReferences) {
        const resourceId = resourceReference.slice('resolveText:'.length);
        if(!canResolveText(resourceId)) {
            continue;
        }
        const translatedText = resolveText(resourceId);
        output = output.replaceAll(resourceReference, translatedText);
    }
    return output;
}