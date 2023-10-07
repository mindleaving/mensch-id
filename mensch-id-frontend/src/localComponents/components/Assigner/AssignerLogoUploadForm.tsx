import { FormEvent, useContext, useState } from "react";
import { Alert, Col, Form, FormGroup, FormLabel, ProgressBar, Row } from "react-bootstrap";
import { uploadFile } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { apiClient } from "../../../sharedCommonComponents/communication/ApiClient";
import { FileUpload } from "../../../sharedCommonComponents/components/FormControls/FileUpload";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { ViewModels } from "../../types/viewModels";
import { loadObject } from "../../../sharedCommonComponents/helpers/LoadingHelpers";
import UserContext from "../../contexts/UserContext";
import { showErrorAlert, showSuccessAlert } from "../../../sharedCommonComponents/helpers/AlertHelpers";
import { DeleteButton } from "../../../sharedCommonComponents/components/DeleteButon";
import { deleteObject } from "../../../sharedCommonComponents/helpers/DeleteHelpers";
import { Center } from "../../../sharedCommonComponents/components/Center";

interface AssignerLogoUploadFormProps {
    onAccountChanged: (userViewModel: ViewModels.AssignerAccountViewModel) => void;
}

export const AssignerLogoUploadForm = (props: AssignerLogoUploadFormProps) => {

    const { onAccountChanged } = props;

    const user = useContext(UserContext)! as ViewModels.AssignerAccountViewModel;
    const [ file, setFile ] = useState<File>();
    const [ isUploading, setIsUploading ] = useState<boolean>(false);
    const [ isDeleting, setIsDeleting ] = useState<boolean>(false);
    const [ uploadProgress, setUploadProgress ] = useState<number>(0);

    const upload = async (e?: FormEvent) => {
        e?.preventDefault();
        if(!file) {
            return;
        }
        setIsUploading(true);
        try {
            await uploadFile(
                file, 
                apiClient.instance!.buildUrl('api/assigner/me/logo'), 
                { 
                    method: 'PUT',
                    contentType: file.type,
                    onProgressChanged: setUploadProgress
                }
            );
            setFile(undefined); 
            showSuccessAlert(resolveText("Assigner_SuccessfullyUploadedLogo"));
    
            try {
                // Reload profile
                await reloadProfile();
            } catch {
                // Ignore
            }
        } catch {
            showErrorAlert(resolveText("Assigner_CouldNotUploadLogo"));
        } finally {
            setIsUploading(false);
        }
    }

    const removeLogo = async () => {
        setIsDeleting(true);
        await deleteObject(
            'api/assigner/me/logo', {},
            resolveText("Assigner_Logo_SuccessfullyRemoved"),
            resolveText("Assigner_Logo_CouldNotRemove"),
            () => {
                reloadProfile();
            },
            undefined,
            () => setIsDeleting(false)
        );
    }

    const reloadProfile = async () => {
        await loadObject(
            'api/assigner/me', {},
            resolveText("Assigner_CouldNotLoadProfile"),
            onAccountChanged
        );
    }

    return (
    <Form onSubmit={upload}>
        <FormGroup as={Row}>
            <FormLabel column>
                {resolveText("Assigner_Logo")}
            </FormLabel>
            <Col xs={2}>
                {user.logoId
                ? <>
                    <img 
                        src={apiClient.instance!.buildUrl(`/api/assigner/${user.accountId}/logo/${user.logoId}`)} 
                        alt="Logo"
                        width='100%'
                    />
                    <Center className="mt-2">
                        <DeleteButton
                            type="button"
                            size="sm"
                            requireConfirm={false}
                            onClick={removeLogo}
                            isDeleting={isDeleting}
                        />
                    </Center>
                </>
                : null}
            </Col>
            <Col>
                {file
                ? <Alert
                    variant="info"
                >
                    {file.name}
                </Alert>
                : <FileUpload
                    onDrop={files => setFile(files[0])}
                    accept={{
                        'image/png': ['.png'],
                        'image/jpg': ['.jpg', '.jpeg' ],
                        'image/gif': ['.gif' ],
                        'image/svg+xml': ['.svg' ],
                    }}
                    maxFiles={1}
                    disabled={isUploading}
                    validator={file => {
                        const lowerFileName = file.name;
                        if(lowerFileName.endsWith(".png") 
                            || lowerFileName.endsWith(".jpg")
                            || lowerFileName.endsWith(".jpeg")
                            || lowerFileName.endsWith(".gif")
                            || lowerFileName.endsWith(".svg")) {
                            return null;
                        }
                        return {
                            code: 'unsupported-imagetype',
                            message: resolveText("Logo_UnsupportedImageType")
                        };
                    }}
                />}
                {isUploading
                ? <div>
                    <ProgressBar
                        now={uploadProgress}
                        variant="success"
                    />
                </div> : null}
            </Col>
            <Col xs="auto">
                <AsyncButton
                    type="submit"
                    isExecuting={isUploading}
                    activeText={resolveText("Upload")}
                    executingText={resolveText("Uploading...")}
                />
            </Col>
        </FormGroup>
    </Form>);

}