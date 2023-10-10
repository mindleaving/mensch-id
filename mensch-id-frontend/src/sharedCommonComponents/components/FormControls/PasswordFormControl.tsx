import { useState } from 'react';
import { Button, FormControl, FormControlProps, InputGroup } from 'react-bootstrap';

interface PasswordFormControlProps extends FormControlProps {
    required?: boolean;
    minLength?: number;
    name?: string;
    min?: number;
    max?: number;
}

export const PasswordFormControl = (props: PasswordFormControlProps) => {

    const [ showPassword, setShowPassword ] = useState<boolean>(false);

    return (
        <InputGroup>
            <FormControl
                {...props}
                type={showPassword ? 'text' : 'password'}
            />
            <Button
                variant={showPassword ? 'outline-danger' : 'outline-success'}
                onClick={() => setShowPassword(state => !state)}
            >
                <i className={showPassword ? 'fa fa-eye-slash red' : 'fa fa-eye green'} />
            </Button>
        </InputGroup>
    );

}
export default PasswordFormControl;