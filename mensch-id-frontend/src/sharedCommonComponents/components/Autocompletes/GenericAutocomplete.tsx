import React, { useMemo } from 'react';
import { Alert } from 'react-bootstrap';
import { Autocomplete } from './Autocomplete';
import { AutocompleteRunner } from '../../helpers/AutocompleteRunner';
import { resolveText } from '../../helpers/Globalizer';

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

export const GenericAutocomplete = <T extends unknown>(props: GenericAutocompleteProps<T>) => {

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
    const autocompleteRunner = useMemo(() => new AutocompleteRunner<T>(apiPath, 'searchText', 10, orderBy, additionalParameters), [ apiPath ]);

    if(value || isLoading) {
        return (<Alert 
            variant="info"
            dismissible={!disabled}
            onClose={() => onChange(undefined)}
        >
            {isLoading 
            ? resolveText('Loading...') 
            : displayFunc(value!)}
        </Alert>);
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