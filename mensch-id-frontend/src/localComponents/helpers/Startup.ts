import { ApiClient, apiClient } from "../../sharedCommonComponents/communication/ApiClient"
import { defaultGlobalizer, Globalizer } from "../../sharedCommonComponents/helpers/Globalizer";
import germanTranslation from '../resources/translation.de.json';
import englishTranslation from '../resources/translation.en.json';

export const initializeApp = () => {
    defaultGlobalizer.instance = new Globalizer(
        navigator.languages.map(x => x.substring(0, 2)), 
        "en", 
        [germanTranslation, englishTranslation]);
    apiClient.instance = window.location.hostname.toLowerCase() === "localhost"
        ? new ApiClient(window.location.hostname, 44321)
        : new ApiClient(window.location.hostname, 443);
    
    apiClient.instance!.defaultOptions.includeCredentials = true;
}