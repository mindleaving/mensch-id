import React, { useMemo, useState } from 'react';
import { Accordion, FormGroup, FormLabel } from 'react-bootstrap';
import { apiClient } from '../../sharedCommonComponents/communication/ApiClient';
import { AccordionCard } from '../../sharedCommonComponents/components/AccordionCard';
import { NotificationManager } from 'react-notifications';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { Models } from '../types/models';
import { format } from 'date-fns';
import { DeleteButton } from '../../sharedCommonComponents/components/DeleteButon';
import { Autocomplete } from '../../sharedCommonComponents/components/Autocompletes/Autocomplete';
import { AutocompleteRunner } from '../../sharedCommonComponents/helpers/AutocompleteRunner';
import { ChallengeReceiverCard } from '../components/ChallengeReceiverCard';

interface MyChallengesPageProps {}

export const MyChallengesPage = (props: MyChallengesPageProps) => {

    const challengeAutoCompleteRunner = useMemo(() => new AutocompleteRunner<Models.MenschIdChallenge>('api/challenges', 'searchText', 10), []);
    const [ challenges, setChallenges ] = useState<Models.MenschIdChallenge[]>([]);
    const [ isDeleting, setIsDeleting ] = useState<boolean>(false);

    const deleteChallenge = async (challengeId: string) => {
        setIsDeleting(true);
        try {
            await apiClient.instance!.delete(`api/challenges/${challengeId}`, {});
            setChallenges(state => state.filter(x => x.id !== challengeId));
            NotificationManager.success(resolveText("Challenge_SuccessfullyDeleted"));
        } catch(error: any) {
            NotificationManager.error(error.message, resolveText("Challenge_CouldNotDelete"));
        } finally {
            setIsDeleting(false);
        }
    }
    return (
        <>
            <h1>{resolveText("MyChallenges")}</h1>
            <FormGroup className='my-3'>
                <FormLabel>{resolveText("Search")}</FormLabel>
                <Autocomplete
                    search={challengeAutoCompleteRunner.search}
                    displayNameSelector={x => x.challengeShortId}
                    onItemSelected={challenge => setChallenges([ challenge ])}
                />
            </FormGroup>
            <Accordion className='mt-3' activeKey={challenges.length > 0 ? challenges[0].id : undefined}>
                {challenges.map(challenge => (
                    <AccordionCard
                        key={challenge.id}
                        eventKey={challenge.id}
                        title={<div className='d-flex align-items-center'>
                            <div className='me-3'>{format(new Date(challenge.createdTimestamp), 'yyyy-MM-dd HH:mm')}</div>
                            -
                            <div className='mx-3'><strong>{challenge.challengeShortId}</strong></div>
                        </div>}
                    >
                        <ChallengeReceiverCard challengeData={challenge} />
                        <DeleteButton
                            requireConfirm={false}
                            onClick={() => deleteChallenge(challenge.id)}
                            isDeleting={isDeleting}
                        />
                    </AccordionCard>
                ))}
            </Accordion>
        </>
    );

}