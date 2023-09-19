import React from 'react';
import Flatpickr from 'react-flatpickr';

interface DateRangeFormControlProps {
    value?: Date[];
    onChange: (startDate: Date | undefined, endDate: Date | undefined) => void;
    disabled?: boolean;
    required?: boolean;
    enableTime?: boolean;
    noCalendar?: boolean;
    triggerOnChangeForUndefinedRange?: boolean;
}

export const DateRangeFormControl = (props: DateRangeFormControlProps) => {

    return (
        <Flatpickr
            options={{
                allowInput: true,
                noCalendar: props.noCalendar,
                enableTime: props.enableTime,
                time_24hr: true,
                mode: 'range'
            }}
            className="form-control"
            required={props.required}
            disabled={props.disabled}
            value={props.value}
            onChange={(dates) => {
                if(dates.length === 2) { 
                    props.onChange(dates[0].toISOString() as any, dates[1].toISOString() as any); 
                } else if(props.triggerOnChangeForUndefinedRange) {
                    props.onChange(undefined, undefined);
                }
            }} 
        />
    );

}