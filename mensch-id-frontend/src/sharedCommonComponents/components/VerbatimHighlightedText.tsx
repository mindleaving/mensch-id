import { useMemo } from "react";
import { HighlightedText } from "./HighlightedText";

interface VerbatimHighlightedTextProps {
    text: string;
    highlightedSegments: string[];
    caseSensitive?: boolean;
    highlightedClassName?: string;
    allowEmptyLines?: boolean;
}

export const VerbatimHighlightedText = (props: VerbatimHighlightedTextProps) => {

    const { 
        text, 
        highlightedSegments, 
        caseSensitive,
        highlightedClassName,
        allowEmptyLines 
    } = props;

    const lines = useMemo(() => text.split('\n').map(str => str.trim()), [ text ]);
    const paragraphs = allowEmptyLines ? lines : lines.filter(x => x.length > 0);

    return (
    <>
        {paragraphs.map((paragraph,idx) => {
            if(paragraph.length === 0) {
                return (<br key={idx} />);
            }
            return (<div key={idx} className='m-0'>
                <HighlightedText
                    text={paragraph}
                    highlightedSegments={highlightedSegments}
                    caseSensitive={caseSensitive}
                    highlightedClassName={highlightedClassName}
                />
            </div>);
        })}
    </>);

}