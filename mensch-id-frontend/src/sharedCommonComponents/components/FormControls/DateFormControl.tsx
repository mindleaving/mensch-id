import React from 'react';
import Flatpickr from 'react-flatpickr';

interface DateFormControlProps {
    id?: string;
    name?: string;
    value?: Date,
    onChange: (date: Date | undefined) => void;
    disabled?: boolean;
    required?: boolean;
    enableTime?: boolean
}

export const DateFormControl = (props: DateFormControlProps) => {

    return (
        <Flatpickr
            options={{
                allowInput: true,
                enableTime: props.enableTime,
                time_24hr: true,
                mode: 'single'
            }}
            id={props.id}
            name={props.name}
            className="form-control"
            required={props.required}
            disabled={props.disabled}
            value={props.value}
            onChange={(dates) => {
                if(dates.length > 0) { 
                    props.onChange(dates[0].toISOString() as any); 
                } else {
                    props.onChange(undefined);
                }
            }} 
        />
    );

}