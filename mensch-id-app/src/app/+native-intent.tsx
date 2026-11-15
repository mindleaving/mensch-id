import { resolveSystemPath } from '../helpers/LinkingHelpers';

export const redirectSystemPath = (event: { path: string; initial: boolean }): string | null => {
    try {
        return resolveSystemPath(event.path, event.initial);
    } catch {
        return event.initial ? '/' : null;
    }
};
