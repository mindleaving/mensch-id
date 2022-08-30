import { KeyboardEvent, useEffect, useState } from 'react';
import Autosuggest from 'react-autosuggest';
import { defaultGlobalizer, resolveText } from '../../helpers/Globalizer';
import '../../styles/react-autosuggest.css';

interface AutocompleteProps<T> {
    defaultValue?: string;
    displayNameSelector: (item: T) => string;
    search: (searchText: string) => Promise<T[]>;
    onItemSelected: (item: T) => void;
    onChange?: (value: string) => void;
    onKeyUp?: (keyEvent: KeyboardEvent<HTMLElement>, currentSearchText: string) => void;
    resetOnSelect?: boolean;
    searchDelayInMilliseconds?: number;
    minSearchTextLength?: number;
    placeholder?: string;
    disabled?: boolean;
    required?: boolean;
    className?: string;
    autoFocus?: boolean;
}

export const Autocomplete = <T extends unknown>(props: AutocompleteProps<T>) => {

    const [ searchTimer, setSearchTimer ] = useState<NodeJS.Timeout>();
    const [ searchText, setSearchText ] = useState<string>(props.defaultValue ?? '');
    const [ suggestions, setSuggestions ] = useState<T[]>([]);

    useEffect(() => {
        if(props.defaultValue) {
            setSearchText(props.defaultValue);
        }
    }, [ props.defaultValue]);

    const onItemSelected = (item: any) => {
        if(props.onItemSelected) {
            props.onItemSelected(item);
        }
        if(props.resetOnSelect) {
            setSearchText('');
            setSuggestions([]);
        } else {
            const itemDisplayName = props.displayNameSelector ? props.displayNameSelector(item) : item;
            setSearchText(itemDisplayName);
        }
    }

    const runSearch = (searchText: string) => {
        const localSearchText = searchText.trim();
        const search = async () => {
            const searchResults = await props.search(localSearchText);
            setSuggestions(searchResults);
        };
        if(props.searchDelayInMilliseconds && props.searchDelayInMilliseconds <= 0) {
            search();
            return;
        }
        if(searchTimer) {
            clearTimeout(searchTimer);
        }
        const searchTimerHandle = setTimeout(search, props.searchDelayInMilliseconds ?? 500);
        setSearchTimer(searchTimerHandle);
    }

    const keyPressed = (keyEvent: KeyboardEvent<HTMLElement>) => {
        if(!props.onKeyUp) {
            return;
        }
        props.onKeyUp(keyEvent, searchText);
    }

    return (
        <Autosuggest
            getSuggestionValue={props.displayNameSelector}
            suggestions={suggestions}
            inputProps={{
                value: searchText,
                onChange: (_e, { newValue }) => {
                    setSearchText(newValue);
                    if(props.onChange) {
                        props.onChange(newValue);
                    }
                },
                onKeyUp: keyPressed,
                placeholder: props.placeholder ?? (defaultGlobalizer.instance ? resolveText('Search') : 'Enter search text'),
                className: `form-control ${props.className ?? ''}`,
                disabled: props.disabled,
                required: props.required,
                autoFocus: props.autoFocus
            }}
            renderSuggestion={item => (
                <div>{props.displayNameSelector(item)}</div>
            )}
            onSuggestionsFetchRequested={({ value }) => runSearch(value)}
            onSuggestionsClearRequested={() => setSuggestions([])}
            shouldRenderSuggestions={(value) => value ? value.trim().length >= (props.minSearchTextLength ?? 3) : false}
            onSuggestionSelected={(_e, { suggestion }) => onItemSelected(suggestion)}
        />
    );

}