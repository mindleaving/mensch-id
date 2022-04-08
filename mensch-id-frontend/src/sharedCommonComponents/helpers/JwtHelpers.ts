export const extractJwtBody = (accessToken: string) => {
    return JSON.parse(atob(accessToken.split('.')[1])) as JsonWebToken;
}