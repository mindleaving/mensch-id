export const removeSensitiveQueryParameters = (): void => {
    if(!window.location.search && !window.location.hash) {
        return;
    }
    window.history.replaceState(null, '', window.location.pathname);
};
