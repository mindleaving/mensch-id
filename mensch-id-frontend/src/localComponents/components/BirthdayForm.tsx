import React, { FormEvent, useState } from 'react';
import { Form, FormGroup, FormLabel } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { loadObject } from '../../sharedCommonComponents/helpers/LoadingHelpers';
import { ViewModels } from '../types/viewModels';
import { BirthDateFormControl } from './BirthDateFormControl';

interface BirthdayFormProps {
    onNewProfileCreated: (birthDate: string, newProfile: ViewModels.NewProfileViewModel) => void;
}

export const BirthdayForm = (props: BirthdayFormProps) => {

    const [ birthDate, setBirthDate ] = useState<string>('');
    const [ isLoading, setIsLoading ] = useState<boolean>(false);

    const loadIdReservations = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsLoading(true);
        await loadObject<ViewModels.NewProfileViewModel>(
            `api/profiles/new`, { birthDate: birthDate },
            resolveText('NewProfile_CouldNotLoad'),
            vm => {
                props.onNewProfileCreated(birthDate, vm);
            },
            undefined,
            () => setIsLoading(false)
        );
    }

    return (
        <Form onSubmit={loadIdReservations}>
            <FormGroup>
                <FormLabel>{resolveText("NewProfile_BirthDate")}</FormLabel>
                <BirthDateFormControl
                    value={birthDate}
                    onChange={setBirthDate}
                />
            </FormGroup>
            <AsyncButton
                className='m-2'
                type='submit'
                activeText={resolveText("Submit")}
                executingText={resolveText("Submitting...")}
                isExecuting={isLoading}
            />
        </Form>
    );

}