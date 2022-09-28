import React, { FormEvent, useState } from 'react';
import { Alert, Col, Form, FormCheck, FormControl, FormGroup, FormLabel, Row } from 'react-bootstrap';
import { AsyncButton } from '../../sharedCommonComponents/components/AsyncButton';
import { RowFormGroup } from '../../sharedCommonComponents/components/RowFormGroup';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { sendPostRequest } from '../../sharedCommonComponents/helpers/StoringHelpers';
import { Models } from '../types/models';
import { NotificationManager } from 'react-notifications';
import { Center } from '../../sharedCommonComponents/components/Center';
import { AccountType } from '../types/enums.d';
import { confirmAlert } from 'react-confirm-alert';
import { Link, useNavigate } from 'react-router-dom';

interface RegistrationFormProps {
    onLoggedIn: (authenticationResult: Models.AuthenticationResult) => void;
}

export const RegistrationForm = (props: RegistrationFormProps) => {

    const [ selectedAccountType, setSelectedAccountType ] = useState<AccountType>(AccountType.Local);
    const [ email, setEmail ] = useState<string>('');
    const [ password, setPassword ] = useState<string>('');
    const [ passwordRepeat, setPasswordRepeat ] = useState<string>('');
    const [ acceptPrivacy, setAcceptPrivacy ] = useState<boolean>(false);
    const [ acceptTermsOfService, setAcceptTermsOfService ] = useState<boolean>(false);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);
    const [ hasBeenRegistered, setHasBeenRegistered ] = useState<boolean>(false);
    const navigate = useNavigate();

    const validate = async (e?: FormEvent) => {
        e?.preventDefault();
        if(password !== passwordRepeat) {
            NotificationManager.error(resolveText("Register_PasswordsDoNotMatch"));
            return;
        }
        if(selectedAccountType === AccountType.LocalAnonymous) {
            confirmAlert({
                title: resolveText("Register_ConfirmAnonymous_Title"),
                message: resolveText("Register_ConfirmAnonymous_Message"),
                closeOnClickOutside: false,
                closeOnEscape: true,
                buttons: [
                    {
                        label: resolveText("Register_ConfirmAnonymous_No"),
                        onClick: () => {}
                    },
                    {
                        label: resolveText("Register_ConfirmAnonymous_Yes"),
                        onClick: register
                    }
                ]
            });
        } else {
            register();
        }
    }
    const register = async () => {
        setIsSubmitting(true);
        const registrationInformation: Models.RegistrationInformation = {
            accountType: selectedAccountType,
            email: selectedAccountType === AccountType.Local ? email : undefined,
            password: password
        };
        await sendPostRequest(
            'api/accounts/register-local',
            resolveText("Register_CouldNotRegister"),
            registrationInformation,
            async response => {
                setHasBeenRegistered(true);
                if(registrationInformation.accountType === AccountType.LocalAnonymous) {
                    const authenticationResult = await response.json() as Models.AuthenticationResult;
                    props.onLoggedIn(authenticationResult);
                } else {
                    navigate("/");
                }
            },
            undefined,
            () => setIsSubmitting(false)
        );
    }

    if(hasBeenRegistered) {
        const text = selectedAccountType === AccountType.Local ? resolveText("Register_HasBeenRegistered_Local")
            : selectedAccountType === AccountType.LocalAnonymous ? resolveText("Register_HasBeenRegistered_LocalAnonymous")
            : resolveText("Register_HasBeenRegistered_LocalAnonymous");
        return (
            <Alert variant='success'>
                {text}
            </Alert>
        );
    }

    const insertLink = (str: string, linkTarget: string) => {
        const placeholderGroup = str.match(/^(.*)\{(.+)\}(.*)$/);
        if(!placeholderGroup) {
            return str;
        }
        return (<>{placeholderGroup[1]}<Link to={linkTarget} target="_blank">{placeholderGroup[2]}</Link>{placeholderGroup[3]}</>);
    }

    return (
        <Form onSubmit={validate}>
            <FormGroup>
                <FormLabel>{resolveText("LocalAccountType")}</FormLabel>
                {[ AccountType.Local, AccountType.LocalAnonymous ].map(accountType => (
                    <Alert key={accountType}>
                        <Row>
                            <Col xs="auto">
                                <FormCheck
                                    type="radio"
                                    id={accountType}
                                    checked={selectedAccountType === accountType}
                                    onChange={(e:any) => {
                                        if(e.target.checked) {
                                            setSelectedAccountType(accountType);
                                        }
                                    }}
                                />
                            </Col>
                            <Col>
                                <strong>{resolveText(`AccountType_${accountType}`)}</strong>
                                <div>{resolveText(`AccountType_${accountType}_Description`)}</div>
                            </Col>
                        </Row>
                    </Alert>
                ))}
            </FormGroup>
            {selectedAccountType === AccountType.Local
            ? <RowFormGroup required
                type='email'
                label={resolveText("Email")}
                value={email}
                onChange={setEmail}
                isValid={email.includes('@')}
                id="email"
                name="email"
            />
            : null}
            <FormGroup as={Row} className='my-1'>
                <FormLabel column xs={4}>{resolveText("Password")}</FormLabel>
                <Col>
                    <FormControl required
                        type='password'
                        value={password}
                        onChange={(e:any) => setPassword(e.target.value)}
                        isInvalid={password.length > 0 && password.length < 8}
                        id="new-password"
                        name="new-password"
                    />
                    <FormControl.Feedback type='invalid'>{resolveText("Register_TooShort")}</FormControl.Feedback>
                </Col>
            </FormGroup>
            <FormGroup as={Row} className='my-1'>
                <FormLabel column xs={4}>{resolveText("PasswordRepeat")}</FormLabel>
                <Col>
                    <FormControl required
                        type='password'
                        value={passwordRepeat}
                        onChange={(e:any) => setPasswordRepeat(e.target.value)}
                        isInvalid={passwordRepeat.length > 0 && passwordRepeat !== password}
                        id="new-password2"
                        name="new-password2"
                    />
                    <FormControl.Feedback type='invalid'>{resolveText("Register_PasswordsDoNotMatch")}</FormControl.Feedback>
                </Col>
            </FormGroup>
            {selectedAccountType === AccountType.Local
            ? <FormGroup>
                <FormCheck required
                    label={insertLink(resolveText("Register_AcceptPrivacy"), "/privacy")}
                    checked={acceptPrivacy}
                    onChange={(e:any) => setAcceptPrivacy(e.target.checked)}
                />
            </FormGroup>
            : null}
            <FormGroup>
                <FormCheck required
                    label={insertLink(resolveText("Register_AcceptTermsOfService"), "/terms-of-service")}
                    checked={acceptTermsOfService}
                    onChange={(e:any) => setAcceptTermsOfService(e.target.checked)}
                />
            </FormGroup>
            <Center>
                <AsyncButton
                    type='submit'
                    className='mt-3'
                    activeText={"Register"}
                    executingText={"Submitting..."}
                    isExecuting={isSubmitting}
                    disabled={
                        password.length === 0 
                        || passwordRepeat !== password 
                        || (selectedAccountType === AccountType.Local && !acceptPrivacy) 
                        || !acceptTermsOfService
                    }
                />
            </Center>
        </Form>
    );

}