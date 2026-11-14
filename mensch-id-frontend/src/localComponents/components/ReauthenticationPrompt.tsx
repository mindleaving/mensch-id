import { Alert, Card } from 'react-bootstrap';
import { Center } from '../../sharedCommonComponents/components/Center';
import { HorizontalLineWithText } from '../../sharedCommonComponents/components/HorizontalLineWithText';
import { showInfoAlert } from '../../sharedCommonComponents/helpers/AlertHelpers';
import { resolveText } from '../../sharedCommonComponents/helpers/Globalizer';
import { buildExternalLoginUrl } from '../helpers/LoginHelpers';
import { ViewModels } from '../types/viewModels';
import { ExternalLogins } from './ExternalLogins';
import { LocalLoginForm } from './LocalLoginForm';

interface ReauthenticationPromptProps {
    availableExternalProviders: ViewModels.LoginProviderViewModel[];
    returnPath: string;
    onReauthenticated: () => void;
}

export const ReauthenticationPrompt = (props: ReauthenticationPromptProps) => {

    const reauthenticateExternally = (loginProviderId: string) => {
        showInfoAlert(resolveText("Redirecting..."));
        const redirectUrl = `https://${window.location.host}${props.returnPath}`;
        window.location.href = buildExternalLoginUrl(loginProviderId, redirectUrl, false);
    };

    return (
        <>
            <Alert variant="warning">{resolveText("Reauthentication_Required")}</Alert>
            <ExternalLogins
                availableExternalProviders={props.availableExternalProviders}
                onProviderSelected={reauthenticateExternally}
            />
            <HorizontalLineWithText text={resolveText("or")} />
            <Center>
                <Card style={{ width: '600px'}}>
                    <Card.Body>
                        <LocalLoginForm
                            onLoggedIn={props.onReauthenticated}
                        />
                    </Card.Body>
                </Card>
            </Center>
        </>
    );

};
export default ReauthenticationPrompt;
