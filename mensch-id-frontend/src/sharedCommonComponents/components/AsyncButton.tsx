import { ReactNode } from "react"
import { Button } from "react-bootstrap";

interface AsyncButtonProps {
    type?: "button" | "submit" | "reset" | undefined;
    form?: string;
    onClick?: React.MouseEventHandler<HTMLButtonElement>;
    variant?: string;
    size?: "sm" | "lg";
    isExecuting?: boolean;
    activeText: ReactNode;
    executingText?: ReactNode;
    className?: string;
    disabled?: boolean;
    title?: string;
}

export const AsyncButton = (props: AsyncButtonProps) => {
    return (
        <Button
            type={props.type}
            form={props.form}
            className={props.className}
            variant={props.variant}
            size={props.size}
            onClick={props.onClick}
            disabled={props.isExecuting || props.disabled}
            title={props.title}
        >
            {!props.isExecuting
                ? <>{props.activeText}</>
                : <><span className="spinner-border spinner-border-sm" role="status" aria-hidden="true" /> {props.executingText ?? props.activeText}</>
            }
        </Button>
    )
}