import React, { FormEvent, useState } from 'react';
import { Button, Card, Col, Form, FormControl, FormGroup, FormLabel, Row } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { ChallengeSenderCard } from '../components/ChallengeSenderCard';
import { Models } from '../types/models';

interface SendChallengePageProps {}

export const SendChallengePage = (props: SendChallengePageProps) => {

    const [ menschId, setMenschId ] = useState<string>('');
    const [ showSettings, setShowSettings ] = useState<boolean>(false);
    const [ challengeLength, setChallengeLength ] = useState<number>(6);
    const [ isSending, setIsSending ] = useState<boolean>(false);
    const [ challengeData, setChallengeData ] = useState<Models.MenschIdChallenge>();

    const sendChallenge = async (e?: FormEvent) => {
        e?.preventDefault();

        setIsSending(true);
        await sendPostRequest(
            `api/id/${menschId}/challenge?challengeLength=${challengeLength}`,
            resolveText("Challenge_CouldNotSend"),
            null,
            async response => {
                const result = await response.json() as Models.MenschIdChallenge;
                setChallengeData(result);
            },
            undefined,
            () => setIsSending(false)
        )
    }

    const toggleSettings = () => {
        setShowSettings(state => !state);
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
                        <FormControl required
                            value={menschId}
                            onChange={(e:any) => setMenschId(e.target.value.toUpperCase())}
                            pattern="[0-9]{8}-[A-Z0-9]{5}"
                            placeholder={resolveText("MenschIDFormatPlaceholder")}
                            size="lg"
                        />
                    </FormGroup>
                    {showSettings
                    ? <FormGroup className='my-3'>
                        <FormLabel>{resolveText("Challenge_Complexity")}</FormLabel>
                        <FormControl
                            as="select"
                            value={challengeLength}
                            onChange={(e:any) => setChallengeLength(e.target.value)}
                        >
                            <option value="4">{resolveText("Challenge_Complexity_Low")} (4)</option>
                            <option value="6">{resolveText("Challenge_Complexity_Medium")} (6)</option>
                            <option value="10">{resolveText("Challenge_Complexity_High")} (10)</option>
                            <option value="18">{resolveText("Challenge_Complexity_Extreme")} (18)</option>
                        </FormControl>
                    </FormGroup> : null}
                    <Row className='align-items-center'>
                        <Col xs="auto">
                            <AsyncButton
                                type="submit"
                                className='m-3'
                                size='lg'
                                activeText={resolveText("Submit")}
                                executingText={resolveText("Submitting...")}
                                disabled={!/[0-9]{8}-[A-Z0-9]{5}/.test(menschId)}
                                isExecuting={isSending}
                            />
                        </Col>
                        <Col xs="auto">
                            <Button
                                variant="link"
                                onClick={toggleSettings}
                                size='lg'
                            >
                                {resolveText("Challenge_Settings")}...
                            </Button>
                        </Col>
                        <Col />
                    </Row>
                </Form>
                </Card.Body>
            </Card>}
        </>
    );

}