import { Alert } from "react-bootstrap";
import { resolveText } from "../helpers/Globalizer";

interface MissingIdAlertProps {}

export const MissingIdAlert = (props: MissingIdAlertProps) => {

    return (<Alert
        variant="danger"
    >
        {resolveText("MissingID")}
    </Alert>);

}