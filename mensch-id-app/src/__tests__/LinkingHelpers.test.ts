import { isAuthRedirect, resolveSystemPath } from '../helpers/LinkingHelpers';

describe('isAuthRedirect', () => {
    it.each([
        'menschid://auth/callback?code=abc&state=xyz',
        'menschid:///auth/callback?code=abc',
        '/auth/callback?code=abc',
        'auth/callback',
        'menschid://auth/logout',
        '/auth/logout/',
        'https://mensch.id/app/auth/callback?code=abc&state=xyz',
        'https://test.mensch.id/app/auth/logout',
        '/app/auth/callback?code=abc'
    ])('recognizes %p', (pathOrUrl) => {
        expect(isAuthRedirect(pathOrUrl)).toBe(true);
    });

    it.each([
        '/',
        '/scan',
        'menschid://create-challenge',
        '/auth/callbacks',
        '/other/auth/callback',
        'https://mensch.id/app/other',
        'https://mensch.id/auth'
    ])('ignores %p', (pathOrUrl) => {
        expect(isAuthRedirect(pathOrUrl)).toBe(false);
    });
});

describe('resolveSystemPath', () => {
    it('suppresses navigation for auth redirects while the app is running', () => {
        expect(resolveSystemPath('menschid:///auth/callback?code=abc', false)).toBeNull();
    });

    it('opens the home screen for auth redirects that start the app', () => {
        expect(resolveSystemPath('menschid:///auth/callback?code=abc', true)).toBe('/');
    });

    it('passes other links through unchanged', () => {
        expect(resolveSystemPath('/scan', false)).toBe('/scan');
    });
});
