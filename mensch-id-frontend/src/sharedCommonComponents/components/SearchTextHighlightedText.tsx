import { useMemo } from "react";
import { HighlightedText } from "./HighlightedText";

interface SearchTextHighlightedTextProps {
    text: string;
    searchText: string;
    caseSensitive?: boolean;
    highlightedClassName?: string;
}

export const SearchTextHighlightedText = (props: SearchTextHighlightedTextProps) => {

    const { text, searchText, caseSensitive, highlightedClassName } = props;

    const searchTerms = useMemo(() => {
        return searchText.split(' ')
            .map(term => term.trim())
            .filter(term => term.length > 0)
            .map(term => caseSensitive ? term : term.toLowerCase());
    }, [ searchText, caseSensitive ]);
    
    return (<HighlightedText
        text={text}
        highlightedSegments={searchTerms}
        caseSensitive={caseSensitive}
        highlightedClassName={highlightedClassName}
    />);
    

}