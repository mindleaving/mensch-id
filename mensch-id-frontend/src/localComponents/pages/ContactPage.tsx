import { Alert } from 'react-bootstrap';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';

interface ContactPageProps {}

export const ContactPage = (props: ContactPageProps) => {

    return (
        <>
            <h1>{resolveText("Contact")}</h1>
            <Alert variant="info" className='mt-3'>
                <div className='d-flex'>
                    <div className='mx-3'>Jan Scholtyssek</div> 
                    | <div className='mx-3'>웃ID: 19891117-XMWT3</div> 
                    | <div className='mx-3'><a href="mailto:mindleaving@gmail.com">{resolveText("Email")}</a></div>
                </div>
            </Alert>
        </>
    );

}