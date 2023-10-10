import math from "mathjs";
import { useState } from "react";
import Form from "react-bootstrap/Form";
import { mathjs } from "../../helpers/mathjs";

interface UnitValueFormControlProps {
    defaultValue?: math.Unit;
    onChange: (unitValue: math.Unit | undefined) => void;
    placeholder?: string;
    className?: string;
    required?: boolean;
}

export const UnitValueFormControl = (props: UnitValueFormControlProps) => {

    const defaultValue = props.defaultValue;
    const [isValid, setIsValid ] = useState<boolean>(true);

    const checkValue = (str: string) => {
        if(str.trim().length === 0) {
            setIsValid(true);
            props.onChange(undefined);
            return;
        }
        try {
            const unit = mathjs.unit!(str.replace(',', '.'));
            setIsValid(true);
            props.onChange(unit);
        } catch(error: any) {
            setIsValid(false);
            props.onChange(undefined);
        }
    }
    return (
        <Form.Control
            required={props.required}
            className={props.className}
            type="text"
            isInvalid={!isValid}
            defaultValue={defaultValue ? defaultValue.toString() : ''}
            onBlur={(e:any) => checkValue(e.target.value)}
            placeholder={props.placeholder}
        />
    )
}