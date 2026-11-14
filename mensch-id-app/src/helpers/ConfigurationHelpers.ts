const DefaultApiBaseUrl = 'https://test.mensch.id';

export const ApiBaseUrl = (process.env.EXPO_PUBLIC_API_URL ?? DefaultApiBaseUrl).replace(/\/+$/, '');
export const OidcIssuer = `${ApiBaseUrl}/api/oidc`;
export const OidcClientId = 'mensch-id-app';
export const IsCustomSchemeRedirectEnabled = process.env.EXPO_PUBLIC_OIDC_REDIRECT_MODE === 'customScheme';
export const OidcRedirectUri = IsCustomSchemeRedirectEnabled
    ? 'menschid://auth/callback'
    : `${ApiBaseUrl}/app/auth/callback`;
export const OidcPostLogoutRedirectUri = IsCustomSchemeRedirectEnabled
    ? 'menschid://auth/logout'
    : `${ApiBaseUrl}/app/auth/logout`;
export const OidcScopes = [ 'openid', 'offline_access', 'mensch_id_api' ];
export const ChallengeSearchMinimumLength = 2;
export const ChallengeSearchDebounceMilliseconds = 300;
export const DefaultChallengeLength = 6;
