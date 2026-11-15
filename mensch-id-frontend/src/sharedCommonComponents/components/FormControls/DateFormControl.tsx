import Flatpickr, { OptionsType } from 'react-flatpickr';
import { formatDateFormControlValue, isValidDate, toDateOnly } from '../../helpers/DateHelpers';
import { Row, Col, Button } from 'react-bootstrap';
import { resolveText } from '../../helpers/Globalizer';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { format } from 'date-fns';
import { combineCssClasses } from '../../helpers/StylingHelpers';

interface DateFormControlProps {
    id?: string;
    name?: string;
    value?: string,
    onChange: (date: string | undefined) => void;
    disabled?: boolean;
    required?: boolean;
    enableTime?: boolean;
    defaultHour?: number;
    size?: "sm" | "lg";
    static?: boolean;
}

export const DateFormControl = (props: DateFormControlProps) => {
    
    const { 
        id,
        name,
        value,
        onChange: onChangeFromProps,
        disabled,
        required,
        enableTime,
        defaultHour,
        size
    } = props;

    const [ localValue, setLocalValue ] = useState<string>(formatDateFormControlValue(value, enableTime));
    const [ previousValue, setPreviousValue ] = useState(value);
    const [ previousEnableTime, setPreviousEnableTime ] = useState(enableTime);
    if(value !== previousValue || enableTime !== previousEnableTime) {
        setPreviousValue(value);
        setPreviousEnableTime(enableTime);
        setLocalValue(formatDateFormControlValue(value, enableTime));
    }

    const flatpickrOptions = useMemo(() => ({
        allowInput: true,
        enableTime: enableTime,
        time_24hr: true,
        defaultHour: defaultHour,
        mode: 'single',
        static: props.static,
        locale: {
            firstDayOfWeek: 1
        }
    } as OptionsType), [ enableTime, defaultHour, props.static ]);

    const onChange = useCallback((dates: Date[]) => {
        if(dates.length > 0 && isValidDate(dates[0])) {
            const date = dates[0];
            onChangeFromProps(enableTime ? date.toISOString() : toDateOnly(date)); 
        } else {
            onChangeFromProps(undefined);
        }
    }, [ enableTime, onChangeFromProps ]);

    return (
    <Row className='align-items-center'>
        <Col className='pe-1'>
            <Flatpickr
                options={flatpickrOptions}
                id={id}
                name={name}
                className={combineCssClasses([
                    "form-control",
                    size ? `form-control-${size}` : undefined
                ])}
                required={required}
                disabled={disabled}
                value={localValue}
                onChange={(_dates,_dateStr,instance) => setLocalValue(instance.input.value)}
                onClose={onChange}
            />
        </Col>
        <Col xs="auto" className="no-print ps-1">
            <Button
                onClick={() => onChange([ new Date() ])}
                size={size}
            >
                {enableTime ? resolveText("Now") : resolveText("Today")}
            </Button>
        </Col>
    </Row>);

}