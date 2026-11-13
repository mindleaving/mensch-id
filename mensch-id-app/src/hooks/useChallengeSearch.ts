import { useEffect, useState } from 'react';
import { ChallengeSearchDebounceMilliseconds, ChallengeSearchMinimumLength } from '../helpers/ConfigurationHelpers';
import { Models } from '../types/models';
import { ChallengeSearchResult, ChallengeSearchState } from '../types/frontendTypes';

const IdleState: ChallengeSearchState = { isSearching: false, hasError: false, challenges: [] };

export const useChallengeSearch = (
    searchText: string,
    searchChallenges: (searchText: string) => Promise<Models.MenschIdChallenge[]>
): ChallengeSearchState => {
    const [ result, setResult ] = useState<ChallengeSearchResult>();
    const isSearchActive = searchText.length >= ChallengeSearchMinimumLength;

    useEffect(() => {
        if(!isSearchActive) {
            return;
        }
        let isCancelled = false;
        const timeout = setTimeout(async () => {
            try {
                const challenges = await searchChallenges(searchText);
                if(!isCancelled) {
                    setResult({ searchText: searchText, hasError: false, challenges: challenges });
                }
            } catch {
                if(!isCancelled) {
                    setResult({ searchText: searchText, hasError: true, challenges: [] });
                }
            }
        }, ChallengeSearchDebounceMilliseconds);
        return () => {
            isCancelled = true;
            clearTimeout(timeout);
        };
    }, [ searchText, isSearchActive, searchChallenges ]);

    if(!isSearchActive) {
        return IdleState;
    }
    const isCurrent = result?.searchText === searchText;
    return {
        isSearching: !isCurrent,
        hasError: isCurrent && result.hasError,
        challenges: isCurrent ? result.challenges : result?.challenges ?? []
    };
};
