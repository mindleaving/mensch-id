import { useMemo } from 'react';
import { Autocomplete } from './Autocomplete';
import { AutocompleteRunner } from '../../helpers/AutocompleteRunner';
import { resolveText } from '../../helpers/Globalizer';
import { DismissableAlert } from '../DismissableAlert';

export interface GenericAutocompleteImplementationProps<T> {
    isLoading?: boolean;
    value?: T;
    onChange: (item: T | undefined) => void;

    // From AutocompleteProps
    searchDelayInMilliseconds?: number;
    minSearchTextLength?: number;
    placeholder?: string;
    disabled?: boolean;
    required?: boolean;
    className?: string;
    autoFocus?: boolean;
    autoSelectSingleItem?: boolean;
}
export interface GenericAutocompleteProps<T> extends GenericAutocompleteImplementationProps<T> {
    apiPath: string;
    displayFunc: (item: T) => string;
    orderBy?: string;
    additionalParameters?: { [key:string]: string };
}

export const GenericAutocomplete = <T,>(props: GenericAutocompleteProps<T>) => {

    const { 
        isLoading,
        value,
        onChange,
        required,
        disabled,
        autoFocus,
        placeholder,
        className,
        searchDelayInMilliseconds,
        minSearchTextLength,
        autoSelectSingleItem,
        displayFunc,
        apiPath,
        orderBy, 
        additionalParameters 
    } = props;
    const autocompleteRunner = useMemo(() => 
        new AutocompleteRunner<T>(apiPath, 'searchText', 10, orderBy, additionalParameters), 
    [ apiPath, orderBy, additionalParameters ]);

    if(value || isLoading) {
        return (<DismissableAlert
            variant='info'
            isDismissable={!disabled}
            onDismiss={() => onChange(undefined)}
        >
            {isLoading 
            ? resolveText('Loading...') 
            : displayFunc(value!)}
        </DismissableAlert>);
    }
    return (<Autocomplete
        search={autocompleteRunner.search}
        displayNameSelector={displayFunc}
        onItemSelected={onChange}
        searchDelayInMilliseconds={searchDelayInMilliseconds}
        minSearchTextLength={minSearchTextLength}
        placeholder={placeholder}
        disabled={disabled}
        required={required}
        className={className}
        autoFocus={autoFocus}
        autoSelectSingleItem={autoSelectSingleItem}
        resetOnSelect
    />);

}