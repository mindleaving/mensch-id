import { useState, FormEvent } from "react";
import { Button, Form, FormGroup, FormLabel, InputGroup } from "react-bootstrap";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { loadObject } from "../../../sharedCommonComponents/helpers/LoadingHelpers";
import { ViewModels } from "../../types/viewModels";
import { BirthDateFormControl } from "../BirthDateFormControl";

interface NewCandidateFormProps {
    birthDate: string;
    onBirthDateChanged: (str: string) => void;
    onNewIdCandidate: (idCandidate: string) => void;
}
export const NewIdCandidateForm = (props: NewCandidateFormProps) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(false);

    const loadIdCandidate = async (e?: FormEvent) => {
        e?.preventDefault();
        setIsLoading(true);
        await loadObject<ViewModels.NewProfileViewModel>(
            `api/assigner/candidate`, { birthDate: props.birthDate },
            resolveText('NewProfile_CouldNotLoad'),
            vm => {
                props.onNewIdCandidate(vm.idCandidates[0]);
            },
            undefined,
            () => setIsLoading(false)
        );
    }

    return (<Form onSubmit={loadIdCandidate}>
        <FormGroup>
            <FormLabel>{resolveText("NewProfile_BirthDate")}</FormLabel>
            <InputGroup>
                <BirthDateFormControl
                    value={props.birthDate}
                    onChange={props.onBirthDateChanged}
                />
                <Button
                    onClick={() => props.onBirthDateChanged(new Date().toISOString().substring(0,10))}
                >
                    {resolveText("Today")}
                </Button>
            </InputGroup>
        </FormGroup>
        <AsyncButton
            className='m-2'
            type='submit'
            activeText={resolveText("Submit")}
            executingText={resolveText("Submitting...")}
            isExecuting={isLoading}
        />
    </Form>);
}