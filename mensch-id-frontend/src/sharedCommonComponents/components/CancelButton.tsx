import { Button } from "react-bootstrap";
import { resolveText } from "../helpers/Globalizer";
import { ButtonVariant } from "react-bootstrap/esm/types";

interface CancelButtonProps {
    onClick: () => void;
    type?: 'button' | 'submit' | 'reset';
    size?: 'sm' | 'lg';
    variant?: ButtonVariant;
    className?: string;
    disabled?: boolean;
}

export const CancelButton = (props: CancelButtonProps) => {
    return (<Button
        type={props.type}
        onClick={props.onClick}
        size={props.size}
        variant={props.variant ?? 'secondary'}
        className={props.className}
        disabled={props.disabled}
    >
        {resolveText("Cancel")}
    </Button>);

}