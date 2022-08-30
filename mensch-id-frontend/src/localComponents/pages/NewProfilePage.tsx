import React, { useState } from 'react';
import { Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { Center } from '../../sharedCommonComponents/components/Center';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { NewProfileForm } from '../components/NewProfileForm';
import { TakeControlForm } from '../components/TakeControlForm';

interface NewProfilePageProps {}

enum NewProfileMode {
    NewId,
    TakeOverControl
};
export const NewProfilePage = (props: NewProfilePageProps) => {

    const [ mode, setMode ] = useState<NewProfileMode>();
    const navigate = useNavigate();

    if(mode === undefined) {
        return (<Center>
            <div className='d-flex flex-column'>
                <Button
                    onClick={() => setMode(NewProfileMode.TakeOverControl)}
                    className='m-2'
                    size='lg'
                >
                    {resolveText(`NewProfileMode_TakeOverControl`)}
                </Button>
                <Button
                    onClick={() => setMode(NewProfileMode.NewId)}
                    className='m-2'
                    size='lg'
                >
                    {resolveText(`NewProfileMode_NewId`)}
                </Button>
            </div>
        </Center>);
    }
    if(mode === NewProfileMode.NewId) {
        return (<>
            <h1>{resolveText("NewProfile")}</h1>
            <NewProfileForm
                onProfileCreated={() => {
                    navigate("/me");
                }}
            />
        </>);
    }
    if(mode === NewProfileMode.TakeOverControl) {
        return (<>
            <h1>{resolveText("TakeOverControl")}</h1>
            <TakeControlForm />
        </>);
    }
    return null;

}