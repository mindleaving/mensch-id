import { Alert } from "react-bootstrap";
import { resolveText } from "../helpers/Globalizer";

interface RedirectingAlertProps {
    variant?: string;
    className?: string;
}

export const RedirectingAlert = (props: RedirectingAlertProps) => {

    const { variant, className } = props;

    return (<Alert
        variant={variant ?? 'info'}
        className={className}
    >
        {resolveText("Redirecting...")}
    </Alert>);

}