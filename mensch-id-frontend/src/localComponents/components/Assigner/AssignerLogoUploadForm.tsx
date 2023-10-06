import { FormEvent, useState } from "react";
import { Col, Form, FormGroup, FormLabel, ProgressBar, Row } from "react-bootstrap";
import { uploadFile } from "../../../sharedCommonComponents/helpers/StoringHelpers";
import { apiClient } from "../../../sharedCommonComponents/communication/ApiClient";
import { FileUpload } from "../../../sharedCommonComponents/components/FormControls/FileUpload";
import { AsyncButton } from "../../../sharedCommonComponents/components/AsyncButton";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";

interface AssignerLogoUploadFormProps {}

export const AssignerLogoUploadForm = (props: AssignerLogoUploadFormProps) => {

    const {  } = props;

    const [ file, setFile ] = useState<File>();
    const [ isUploading, setIsUploading ] = useState<boolean>(false);
    const [ uploadProgress, setUploadProgress ] = useState<number>(0);

    const upload = async (e?: FormEvent) => {
        if(!file) {
            return;
        }
        setIsUploading(true);
        await uploadFile(
            file, 
            'api/assigner/logo', 
            { 
                method: 'PUT',
                contentType: 'image/png',
                accessToken: apiClient.instance!.accessToken!,
                onProgressChanged: setUploadProgress
            }
        );
        setIsUploading(false);
    }

    return (
    <Form onSubmit={upload}>
        <FormGroup as={Row}>
            <FormLabel column>
                {resolveText("Assigner_Logo")}
            </FormLabel>
            <Col>
                <FileUpload
                    onDrop={files => setFile(files[0])}
                    accept={{
                        'image/png': ['.png'],
                        'image/jpg': ['.jpg', '.jpeg' ],
                    }}
                    maxFiles={1}
                    disabled={isUploading}
                    validator={file => {
                        if(file.name.endsWith(".png") || file.name.endsWith(".jpg")) {
                            return null;
                        }
                        return {
                            code: 'unsupported-imagetype',
                            message: resolveText("Logo_UnsupportedImageType")
                        };
                    }}
                />
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