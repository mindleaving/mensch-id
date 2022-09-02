import React, { ElementType, PropsWithChildren } from 'react';
import { Col, FormControl, FormGroup, FormLabel, Row } from 'react-bootstrap';

interface RowFormGroupProps {
    label: string;
    as?: ElementType<any>;
    type?: string;
    value: any;
    min?: number,
    max?: number,
    onChange: (changedValue: any) => void;
    disabled?: boolean;
    required?: boolean;
    isValid?: boolean;
    isInvalid?: boolean;
}

export const RowFormGroup = (props: PropsWithChildren<RowFormGroupProps>) => {

    return (
        <FormGroup as={Row} className='my-1'>
            <FormLabel column xs={4}>{props.label}</FormLabel>
            <Col>
              <FormControl
                    required={props.required}
                    as={props.as}
                    type={props.type}
                    value={props.value}
                    min={props.min}
                    max={props.max}
                    isValid={props.isValid}
                    isInvalid={props.isInvalid}
                    onChange={(e:any) => props.onChange(e.target.value)}
                    disabled={props.disabled}
                >
                    {props.children}
                </FormControl>
            </Col>
        </FormGroup>
    );

}