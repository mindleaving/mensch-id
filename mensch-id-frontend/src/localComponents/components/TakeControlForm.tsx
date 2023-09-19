import React, { FormEvent, useState } from 'react';
import { Form, FormControl, FormGroup, FormLabel } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { isMenschId } from '../helpers/MenschIdHelpers';
import { formatOwnershipSecret, removeInvalidCharactersFromOwnershipSecret } from '../helpers/OwnershipSecretFormatter';
import { Models } from '../types/models';

interface TakeControlFormProps {}

export const TakeControlForm = (props: TakeControlFormProps) => {

    const [ id, setId ] = useState<string>('');
    const [ ownershipSecret, setOwnershipSecret ] = useState<string>('');
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const navigate = useNavigate();

    const takeControl = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsSubmitting(true);
        const body: Models.TakeControlBody = {
            id: id,
            ownershipSecret: ownershipSecret.replaceAll(/[^A-Z0-9]/g, "")
        };
        await sendPostRequest(
            `api/profiles/take-control`, {},
            resolveText("TakeControl_CouldNotTakeControl"),
            body,
            () => {
                navigate("/me");
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    return (
        <Form onSubmit={takeControl}>
            <FormGroup>
                <FormLabel>{resolveText("Person_ID")}</FormLabel>
                <FormControl
                    value={id}
                    onChange={(e:any) => setId(e.target.value.toUpperCase().trim())}
                    size='lg'
                />
            </FormGroup>
            <FormGroup>
                <FormLabel>{resolveText("TakeControl_OwnershipSecret")}</FormLabel>
                <FormControl
                    value={ownershipSecret}
                    onChange={(e:any) => setOwnershipSecret(formatOwnershipSecret(e.target.value))}
                    size='lg'
                />
            </FormGroup>
            <AsyncButton
                type='submit'
                className='m-3'
                activeText={resolveText("TakeControl_TakeControl")}
                executingText={resolveText("TakeControl_TakingControl")}
                isExecuting={isSubmitting}
                disabled={removeInvalidCharactersFromOwnershipSecret(ownershipSecret).length !== 30 || !isMenschId(id)}
            />
        </Form>
    );

}