type Update<T> = (item: T) => T;
type Group<T> = {
    key: string;
    items: T[];
}
type Groups<T> = Group<T>[];
type Dictionary<T> = { [key:string]: T };
type QueryParameter = { key: string, value: string | undefined };
type QueryParameters = { [key:string]: string | undefined } | QueryParameter[];
interface JsonWebToken {
    id: string;
    nbf: number;
    exp: number;
    iat: number;
    iss: string;
    aud: string;
    [key: string]: string
}
export interface RouteDefinition {
    path: string;
    element: ReactNode;
    usesCustomLayout?: boolean;
};
export interface IdAutocompleteProps {
    value?: string | null;
    onChange: (value: string | undefined) => void;
    disabled?: boolean;
    required?: boolean;
    placeholder?: string;
}
export interface FormStepProps<T> {
    formData: T;
    onChange: (update: Update<T>) => void;
}
export interface FormStep<T> {
    title: string;
    element: (props: FormStepProps<T>) => ReactNode;
    canMovePrevious?: (formData: T) => boolean;
    canMoveNext?: (formData: T) => boolean;
    hideNavigation?: (formData: T) => boolean;
}
export interface CustomFormControlProps {
    required?: boolean;
    readOnly?: boolean;
    disabled?: boolean;
    ariaDescribedBy?: string;
    min?: number;
    max?: number;
    step?: number,
    minLength?: number;
    isValid?: boolean;
    isInvalid?: boolean;
    size?: "sm" | "lg";
}
export interface DistinctItemWithMultiplicity<T> {
    item: T;
    multiplicity: number;
}
export type JsonPatchDocument = {
    op: "add" | "replace"
    path: string;
    value: any
}
| {
    op: "remove",
    path: string;
} 
| {
    op: "copy" | "move",
    from: string;
    path: string;
};
export interface TextFragment {
    text: string;
    isHighlighted: boolean;
}
export interface TimeCluster<T> {
    startTime: Date;
    endTime: Date;
    items: T[];
}
export interface ConfirmAlertOptions {
    title: string;
    message: string;
    closeOnClickOutside?: boolean;
    buttons: {
        label: string;
        onClick?: () => void;
    }[];
}