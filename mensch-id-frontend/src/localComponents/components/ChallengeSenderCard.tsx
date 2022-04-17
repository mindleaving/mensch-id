import React from 'react';
import { Card, Row, Col } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { Models } from '../types/models';

interface ChallengeSenderCardProps {
    challengeData: Models.MenschIdChallenge;
}

export const ChallengeSenderCard = (props: ChallengeSenderCardProps) => {

    return (
        <Card className='m-3'>
            <Card.Header>{resolveText("ChallengeData")}</Card.Header>
            <Card.Body>
                <Row>
                    <Col><h5>웃ID</h5></Col>
                    <Col>{props.challengeData.menschId}</Col>
                </Row>
                <Row className='my-3'>
                    <Col>
                        <h5>{resolveText("Challenge_ID")}</h5>
                        <div><small>{resolveText("SendChallenge_ID_Description")}</small></div>
                    </Col>
                    <Col><h3>{props.challengeData.challengeShortId}</h3></Col>
                </Row>
                <Row className='my-3'>
                    <Col>
                        <h5>{resolveText("Challenge_Secret")}</h5>
                        <div>
                            <span className='red'><strong><small>{resolveText("SendChallenge_Secret_KeepSecret")}</small></strong></span>
                        </div>
                        <div>
                            <small>{resolveText("SendChallenge_Secret_Description")}</small>
                        </div>
                    </Col>
                    <Col><h3>{props.challengeData.challengeSecret}</h3></Col>
                </Row>
            </Card.Body>
        </Card>
    );

}