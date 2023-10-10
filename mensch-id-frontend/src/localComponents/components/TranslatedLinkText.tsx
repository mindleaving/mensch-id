import { Link } from "react-router-dom";

interface TranslatedLinkTextProps {
    translatedTextWithPlaceholder: string;
    linkTarget: string;
}

export const TranslatedLinkText = (props: TranslatedLinkTextProps) => {

    const { translatedTextWithPlaceholder, linkTarget } = props;

    const placeholderGroup = translatedTextWithPlaceholder.match(/^(.*)\{(.+)\}(.*)$/);
    
    if(!placeholderGroup) {
        return <>{translatedTextWithPlaceholder}</>;
    }
    return (<>{placeholderGroup[1]}<Link to={linkTarget} target="_blank">{placeholderGroup[2]}</Link>{placeholderGroup[3]}</>);

}