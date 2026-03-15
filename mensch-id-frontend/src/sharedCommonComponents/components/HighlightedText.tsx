import { useMemo } from "react";
import { TextFragment } from "../types/frontendTypes";

interface HighlightedTextProps {
    text: string;
    highlightedSegments: string[];
    caseSensitive?: boolean;
    highlightedClassName?: string;
}

export const HighlightedText = (props: HighlightedTextProps) => {

    const { text, highlightedSegments: searchTerms, caseSensitive, highlightedClassName } = props;

    const fragments = useMemo(() => {
        const fragments: TextFragment[] = [];
        const normalizedText = caseSensitive ? text : text.toLowerCase();
        let remainingTextOffset = 0;
        while(remainingTextOffset < normalizedText.length) {
            const remainingText = normalizedText.substring(remainingTextOffset);
            const matchedSearchTerms = searchTerms.map(term => {
                const matchPosition = remainingText.indexOf(term);
                return { term, matchPosition };
            }).filter(x => x.matchPosition >= 0);
            if(matchedSearchTerms.length === 0) {
                fragments.push({
                    text: text.substring(remainingTextOffset),
                    isHighlighted: false
                });
                break;
            }
            const firstMatchedPosition = Math.min(...matchedSearchTerms.map(x => x.matchPosition));
            const lengthSortedFirstMatchedTerms = matchedSearchTerms
                .filter(x => x.matchPosition === firstMatchedPosition)
                .sort((a,b) => b.term.length - a.term.length);
            const longestFirstMatch = lengthSortedFirstMatchedTerms[0];
            if(longestFirstMatch.matchPosition > 0) {
                fragments.push({
                    text: text.substring(remainingTextOffset, remainingTextOffset + longestFirstMatch.matchPosition),
                    isHighlighted: false
                });
            }
            const matchStartPosition = remainingTextOffset + longestFirstMatch.matchPosition;
            fragments.push({
                text: text.substring(matchStartPosition, matchStartPosition + longestFirstMatch.term.length),
                isHighlighted: true
            });
            remainingTextOffset += longestFirstMatch.matchPosition + longestFirstMatch.term.length;
        }
        return fragments;
    }, [ text, searchTerms, caseSensitive ]);

    return (<div className="d-inline">
        {fragments.map((fragment,fragmentIndex) => {
            if(fragment.isHighlighted) {
                if(highlightedClassName !== undefined) {
                    return (<span key={fragment.text+fragmentIndex} className={highlightedClassName}>{fragment.text}</span>);
                }
                return (<span key={fragment.text+fragmentIndex}><strong>{fragment.text}</strong></span>);
            } else {
                return (<span key={fragment.text+fragmentIndex}>{fragment.text}</span>);
            }
        })}
    </div>);

}