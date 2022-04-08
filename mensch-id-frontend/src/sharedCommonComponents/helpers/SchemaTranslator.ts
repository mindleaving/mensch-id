import { canResolveText, resolveText } from "./Globalizer";

export const translateSchema = (schema: any) => {
    const typeName = firstLetterToUpper(schema.title);
    translateProperties(typeName, schema.properties);
    translateDefinitions(schema.definitions);
    return schema;
}
const firstLetterToUpper = (str: string) => {
    if(!str) {
        return str;
    }
    return str[0].toUpperCase() + str.substring(1);
}
const translateProperties = (typeName: string, properties: any) => {
    if(!properties) {
        return;
    }
    for (const propertyName in properties) {
        if (Object.prototype.hasOwnProperty.call(properties, propertyName)) {
            const property = properties[propertyName];
            const translationKey = `${typeName}_${firstLetterToUpper(propertyName)}`;
            if(canResolveText(translationKey)) {
                property.title = resolveText(translationKey);
            }
        }
    }
}
const translateDefinitions = (definitions: any) => {
    if(!definitions) {
        return;
    }
    for (const typeName in definitions) {
        if (Object.prototype.hasOwnProperty.call(definitions, typeName)) {
            const definition = definitions[typeName];
            const translationKey = typeName;
            if(canResolveText(translationKey)) {
                definition.title = resolveText(translationKey);
            }
            translateProperties(typeName, definition.properties);
        }
    }
}