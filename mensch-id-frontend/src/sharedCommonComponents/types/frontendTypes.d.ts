type Update<T> = (item: T) => T;
interface JsonWebToken {
    id: string;
    nbf: number;
    exp: number;
    iat: number;
    iss: string;
    aud: string;
    [key: string]: string
}