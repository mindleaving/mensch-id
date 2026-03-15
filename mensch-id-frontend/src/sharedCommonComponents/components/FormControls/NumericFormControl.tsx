import { FormControl } from "react-bootstrap";
import { CustomFormControlProps } from "../../types/frontendTypes";
import { useEffect, useRef, useState } from "react";

interface NumericFormControlProps extends CustomFormControlProps {
    value?: number;
    onChange: (value: number | undefined) => void;
    autoBlur?: boolean;
    autoBlurDelayInMilliseconds?: number;
    precision?: number;
    decimals?: number;
    onIsChangeInProgress?: (isChangeInProgress: boolean) => void;
}

const formatNumber = (value: number, decimals?: number, precision?: number) => {
    if(decimals) {
        return value.toFixed(decimals);
    }
    if(precision) {
        if(value >= Math.pow(10, precision)) {
            return value.toFixed(0);
        }
        return value.toPrecision(precision);
    }
    return value.toFixed(0);
}
export const NumericFormControl = (props: NumericFormControlProps) => {

    const { 
        value, 
        onChange, 
        autoBlur, 
        autoBlurDelayInMilliseconds, 
        precision,
        decimals,
        onIsChangeInProgress
    } = props;

    const ref = useRef<HTMLInputElement>(null);
    const [ inputValue, setInputValue ] = useState<string>(value && !isNaN(value) ? formatNumber(value, decimals, precision) : '');
    const [ autoBlurTimeout, setAutoBlurTimeout ] = useState<number>();

    const blur = () => {
        ref.current?.blur();
        ref.current?.focus();
    }

    const commit = () => {
        if(inputValue.trim().length === 0) {
            onChange(undefined);
            return;
        }
        const parsedNumber = Number(inputValue);
        if(isNaN(parsedNumber)) {
            onChange(undefined);
            return;
        }
        onChange(Number(inputValue));
    }

    useEffect(() => {
        if(value && !isNaN(value)) {
            setInputValue(formatNumber(value, decimals, precision));
        } else {
            setInputValue('');
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [ value ]);

    return (
    <FormControl
        ref={ref}
        type="text"
        value={inputValue}
        onChange={e => setInputValue(e.target.value.replaceAll(',', '.'))}
        onBlur={() => {
            commit();
            if(autoBlurTimeout) {
                clearTimeout(autoBlurTimeout);
            }
            if(onIsChangeInProgress) {
                onIsChangeInProgress(false);
            }
        }}
        min={props.min}
        max={props.max}
        step={props.step}
        required={props.required}
        readOnly={props.readOnly}
        disabled={props.disabled}
        size={props.size}
        isValid={props.isValid}
        isInvalid={props.isInvalid}
        onFocus={e => e.target.select()}
        onKeyDown={e => {
            if(e.key === 'Enter') {
                blur();
                return;
            }
            if(autoBlur) {
                if(autoBlurTimeout) {
                    clearTimeout(autoBlurTimeout);
                }
                const timeout = setTimeout(blur, autoBlurDelayInMilliseconds ?? 1500);
                setAutoBlurTimeout(timeout);
            }
            if(onIsChangeInProgress) {
                onIsChangeInProgress(true);
            }
        }}
    />);

}