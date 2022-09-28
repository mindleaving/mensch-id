interface VerbatimTextProps {
    text: string;
}

export const VerbatimText = (props: VerbatimTextProps) => {

    const paragraphs = props.text.split('\n').filter(x => x.length > 0);
    return (
    <>
        {paragraphs.map((paragraph,idx) => (
            <p key={idx} className='m-0'>
                {paragraph}
            </p>
        ))}
    </>
    );

}