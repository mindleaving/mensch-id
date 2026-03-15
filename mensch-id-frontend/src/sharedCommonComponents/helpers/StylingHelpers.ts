export const combineCssClasses = (classes: (string | null | undefined)[]) => {
    return classes.filter(x => !!x?.trim()).join(' ');
}