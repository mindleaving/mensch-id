const AuthRedirectPaths = [ '/auth/callback', '/auth/logout', '/app/auth/callback', '/app/auth/logout' ];

const extractPath = (pathOrUrl: string): string => {
    const withoutHost = pathOrUrl.replace(/^https?:\/\/[^/?#]*/i, '');
    const withoutScheme = withoutHost.replace(/^[a-z][a-z0-9+.-]*:\/*/i, '/');
    const pathEnd = withoutScheme.search(/[?#]/);
    const path = pathEnd >= 0 ? withoutScheme.substring(0, pathEnd) : withoutScheme;
    return path.startsWith('/') ? path : `/${path}`;
};

export const isAuthRedirect = (pathOrUrl: string): boolean => {
    const path = extractPath(pathOrUrl).replace(/\/+$/, '').toLowerCase();
    return AuthRedirectPaths.includes(path);
};

export const resolveSystemPath = (
    pathOrUrl: string,
    isInitial: boolean
): string | null => {
    if(!isAuthRedirect(pathOrUrl)) {
        return pathOrUrl;
    }
    return isInitial ? '/' : null;
};
