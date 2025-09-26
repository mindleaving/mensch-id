import { DependencyList, useEffect } from "react";

export const defaultWindowTitle: { title?: string } = {};

export function useWindowTitle(titleBuilder: () => string, deps?: DependencyList) {
    useEffect(() => {
        document.title = `${defaultWindowTitle.title} - ${titleBuilder()}`;
        return () => {
            document.title = defaultWindowTitle.title ?? '';
        }
    }, [ titleBuilder, deps ]);
}