export const findUpstreamNodeByAttribute = (startElement: Element, attributeName: string) => {
    if(!startElement) {
        return undefined;
    }
    let nextElement = startElement;
    while(!nextElement.getAttribute(attributeName)) {
        if(!nextElement.parentElement) {
            return undefined;
        }
        nextElement = nextElement.parentElement
    }
    return nextElement;
}