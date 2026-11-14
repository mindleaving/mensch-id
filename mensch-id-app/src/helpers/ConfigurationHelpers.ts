const DefaultApiBaseUrl = 'https://test.mensch.id';

export const ApiBaseUrl = (process.env.EXPO_PUBLIC_API_URL ?? DefaultApiBaseUrl).replace(/\/+$/, '');
export const OidcClientId = 'mensch-id-app';
export const OidcRedirectUri = 'menschid://auth/callback';
export const OidcPostLogoutRedirectUri = 'menschid://auth/logout';
export const OidcScopes = [ 'openid', 'offline_access', 'mensch_id_api' ];
export const ChallengeSearchMinimumLength = 2;
export const ChallengeSearchDebounceMilliseconds = 300;
export const DefaultChallengeLength = 6;
