import { ElementType, PropsWithChildren } from 'react';
import { Col, FormCheck, FormControl, FormGroup, FormLabel, FormSelect, Row } from 'react-bootstrap';
import { DateFormControl } from './DateFormControl';
import PasswordFormControl from './PasswordFormControl';

interface RowFormGroupProps {
    id?: string;
    name?: string;
    label: string;
    as?: ElementType<any>;
    type?: "date" | "datetime" | "checkbox" | "password" | "email" | "number" | "text";
    value: any;
    min?: number,
    max?: number,
    onChange: (changedValue: any) => void;
    disabled?: boolean;
    required?: boolean;
    isValid?: boolean;
    isInvalid?: boolean;
    pattern?: string;
    placeholder?: string;
    className?: string;
}

export const RowFormGroup = (props: PropsWithChildren<RowFormGroupProps>) => {

    return (
        <FormGroup as={Row} className={props.className ?? 'my-1'}>
            <FormLabel column xs={4} lg={6}>{props.label}{props.required ? '*' : null}</FormLabel>
            <Col>
                {props.type?.toLowerCase() === "date" || props.type?.toLowerCase() === "datetime"
                ? <DateFormControl
                    id={props.id}
                    name={props.name}
                    required={props.required}
                    disabled={props.disabled}
                    enableTime={props.type!.toLowerCase() === "datetime"}
                    value={props.value}
                    onChange={props.onChange}
                />
                : props.type?.toLowerCase() === "checkbox"
                ? <FormCheck
                    id={props.id}
                    name={props.name}
                    required={props.required}
                    checked={props.value}
                    onChange={(e:any) => props.onChange(e.target.checked)}
                    disabled={props.disabled}
                    isValid={props.isValid}
                    isInvalid={props.isInvalid}
                />
                : props.type?.toLowerCase() === "password"
                ? <PasswordFormControl
                    required={props.required}
                    id={props.id}
                    name={props.name}
                    value={props.value}
                    min={props.min}
                    max={props.max}
                    isValid={props.isValid}
                    isInvalid={props.isInvalid}
                    onChange={(e:any) => props.onChange(e.target.value)}
                    disabled={props.disabled}
                />
                : props.as === "select"
                ? <FormSelect
                    required={props.required}
                    id={props.id}
                    name={props.name}
                    value={props.value}
                    isValid={props.isValid}
                    isInvalid={props.isInvalid}
                    onChange={(e:any) => props.onChange(e.target.value)}
                    disabled={props.disabled}
                >
                    {props.children}
                </FormSelect>
                : <FormControl
                    required={props.required}
                    as={props.as}
                    type={props.type}
                    id={props.id}
                    name={props.name}
                    value={props.value}
                    min={props.min}
                    max={props.max}
                    pattern={props.pattern}
                    isValid={props.isValid}
                    isInvalid={props.isInvalid}
                    onChange={(e:any) => props.onChange(e.target.value)}
                    disabled={props.disabled}
                    placeholder={props.placeholder}
                >
                    {props.children}
                </FormControl>}
            </Col>
        </FormGroup>
    );

}