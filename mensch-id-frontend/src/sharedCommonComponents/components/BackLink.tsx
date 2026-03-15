import { Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { resolveText } from '../helpers/Globalizer';

interface BackLinkProps {
    target?: string;
}

export const BackLink = (props: BackLinkProps) => {

    const navigate = useNavigate();

    return (
        <Button 
            variant='info'
            onClick={() => props.target ? navigate(props.target) : navigate(-1)}
        >
            &lt; {resolveText("Back")}
        </Button>
    );

}