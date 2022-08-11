interface HorizontalLineWithTextProps {
    text: string;
}

export const HorizontalLineWithText = (props: HorizontalLineWithTextProps) => {

    return (
        <div className="separator-with-text">
            <span className="text-secondary">{props.text}</span>
        </div>
    );

}