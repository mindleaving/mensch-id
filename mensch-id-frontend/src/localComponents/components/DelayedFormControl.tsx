import { useEffect, useRef, useState } from 'react';
import { FormControl } from 'react-bootstrap';

interface DelayedFormControlProps {
    value: string;
    onChange: (value: string) => void;
    delayInMilliseconds?: number;
    placeholder?: string;
    className?: string;
}

export const DelayedFormControl = (props: DelayedFormControlProps) => {

    const { value, onChange, delayInMilliseconds = 300 } = props;
    const [ currentValue, setCurrentValue ] = useState<string>(value);
    const onChangeRef = useRef(onChange);

    useEffect(() => {
        onChangeRef.current = onChange;
    }, [ onChange ]);

    useEffect(() => {
        if(currentValue === value) {
            return;
        }
        const timeout = setTimeout(() => onChangeRef.current(currentValue), delayInMilliseconds);
        return () => clearTimeout(timeout);
    }, [ currentValue, value, delayInMilliseconds ]);

    return (
        <FormControl
            className={props.className}
            placeholder={props.placeholder}
            value={currentValue}
            onChange={e => setCurrentValue(e.target.value)}
        />
    );
};
