interface VerbatimTextProps {
    text: string;
    paragraphClassName?: string;
}

export const VerbatimText = (props: VerbatimTextProps) => {

    const paragraphs = props.text.split('\n').filter(x => x.length > 0);
    return (
    <>
        {paragraphs.map((paragraph,idx) => (
            <p key={idx} className={props.paragraphClassName}>
                {paragraph}
            </p>
        ))}
    </>
    );

}