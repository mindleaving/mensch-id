import { SessionType } from '../types/enums';
import { ViewModels } from '../types/viewModels';

const BrowserPatterns: { name: string; pattern: RegExp }[] = [
    { name: 'Edge', pattern: /Edg\//i },
    { name: 'Opera', pattern: /OPR\//i },
    { name: 'Firefox', pattern: /Firefox\//i },
    { name: 'Chrome', pattern: /Chrome\//i },
    { name: 'Safari', pattern: /Safari\//i }
];

const OperatingSystemPatterns: { name: string; pattern: RegExp }[] = [
    { name: 'Android', pattern: /Android/i },
    { name: 'iOS', pattern: /iPhone|iPad|iPod/i },
    { name: 'Windows', pattern: /Windows/i },
    { name: 'macOS', pattern: /Mac OS X|Macintosh/i },
    { name: 'Linux', pattern: /Linux/i }
];

const findMatchingName = (userAgent: string, patterns: { name: string; pattern: RegExp }[]): string | undefined => {
    return patterns.find(x => x.pattern.test(userAgent))?.name;
};

export const describeUserAgent = (userAgent: string | null | undefined): string | undefined => {
    if(!userAgent) {
        return undefined;
    }
    const browser = findMatchingName(userAgent, BrowserPatterns);
    const operatingSystem = findMatchingName(userAgent, OperatingSystemPatterns);
    if(browser && operatingSystem) {
        return `${browser} (${operatingSystem})`;
    }
    return browser ?? operatingSystem ?? userAgent;
};

export const getSessionRevokePath = (session: ViewModels.SessionViewModel): string => {
    const typeSegment = session.type === SessionType.App ? 'app' : 'web';
    return `api/sessions/${typeSegment}/${encodeURIComponent(session.id)}`;
};

export const getSessionIconClassName = (session: ViewModels.SessionViewModel): string => {
    return session.type === SessionType.App ? 'fa fa-mobile' : 'fa fa-globe';
};
