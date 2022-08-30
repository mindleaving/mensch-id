interface HorizontalLineWithTextProps {
    text: string;
}

export const HorizontalLineWithText = (props: HorizontalLineWithTextProps) => {

    return (
        <div className="timelineSeparator">
            <span className="text-secondary">{props.text}</span>
        </div>
    );

}