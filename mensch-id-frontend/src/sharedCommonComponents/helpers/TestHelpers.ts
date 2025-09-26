export const isEqualWithin = (a: number, b: number, precision: number) => {
    return Math.abs(a - b) < precision;
}