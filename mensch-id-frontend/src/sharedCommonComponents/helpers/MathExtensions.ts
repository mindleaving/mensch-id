export const isBetween = (value: number, from: number, to: number) => {
    const rangeStart = to >= from ? from : to;
    const rangeEnd = to >= from ? to : from;
    return value >= rangeStart && value <= rangeEnd;
}