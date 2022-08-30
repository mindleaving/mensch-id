import React from 'react';
import { FormControl } from 'react-bootstrap';

interface BirthDateFormControlProps {
    value: string;
    onChange: (birthDate: string) => void;
}

export const BirthDateFormControl = (props: BirthDateFormControlProps) => {

    return (
        <FormControl
            value={props.value}
            onChange={(e: any) => props.onChange(e.target.value)}
            pattern='[0-9]{4}-[0-9]{2}-[0-9]{2}'
            placeholder='Format: yyyy-MM-dd'
        />
    );

}