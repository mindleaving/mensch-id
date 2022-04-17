import React, { FormEvent, useState } from 'react';
import { Button, Card, Form, FormControl, FormGroup, FormLabel } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { ChallengeSenderCard } from '../components/ChallengeSenderCard';
import { Models } from '../types/models';

interface SendChallengePageProps {}

export const SendChallengePage = (props: SendChallengePageProps) => {

    const [ menschId, setMenschId ] = useState<string>('');
    const [ isSending, setIsSending ] = useState<boolean>(false);
    const [ challengeData, setChallengeData ] = useState<Models.MenschIdChallenge>();

    const sendChallenge = async (e?: FormEvent) => {
        e?.preventDefault();

        setIsSending(true);
        await sendPostRequest(
            `api/id/${menschId}/challenge`,
            resolveText("Challenge_CouldNotSend"),
            null,
            async response => {
                const result = await response.json() as Models.MenschIdChallenge;
                setChallengeData(result);
            },
            () => setIsSending(false)
        )
    }
    return (
        <>
            <h1>{resolveText("SendChallenge")}</h1>
            <p className='lead'>
                {resolveText("SendChallenge_Text")}
            </p>
            {!!challengeData
            ? <>
                <ChallengeSenderCard challengeData={challengeData} />
                <Button onClick={() => setChallengeData(undefined)}>{resolveText("NewChallenge")}</Button>
            </>
            : <Card>
                <Card.Body>
                <Form onSubmit={sendChallenge}>
                    <FormGroup>
                        <FormLabel>웃ID</FormLabel>
                        <FormControl
                            value={menschId}
                            onChange={(e:any) => setMenschId(e.target.value.toUpperCase())}
                            pattern="[0-9]{8}-[A-Z0-9]{5}"
                            placeholder='Format: yyyyMMdd-XXXX'
                        />
                    </FormGroup>
                    <AsyncButton
                        type="submit"
                        className='m-3'
                        activeText={resolveText("Submit")}
                        executingText={resolveText("Submitting...")}
                        isExecuting={isSending}
                    />
                </Form>
                </Card.Body>
            </Card>}
        </>
    );

}