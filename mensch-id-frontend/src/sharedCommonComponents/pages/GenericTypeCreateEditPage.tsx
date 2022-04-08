import { useEffect, useState } from 'react';
import { resolveText } from '../helpers/Globalizer';
import { buildLoadObjectFunc } from '../helpers/LoadingHelpers';
import { Form } from '@rjsf/bootstrap-4';
import { AsyncButton } from '../components/AsyncButton';
import { NotificationManager } from 'react-notifications';
import { IChangeEvent } from '@rjsf/core';
import { useParams } from 'react-router-dom';
import { translateSchema } from '../helpers/SchemaTranslator';
import { v4 as uuid } from 'uuid';
import { AccordionArrayFieldTemplate } from '../components/AccordionArrayFieldTemplate';

interface GenericTypeCreateEditPageProps<T> {
    typeName: string;
    paramName?: string;
    item?: T;
    itemLoader?: (id: string) => Promise<T>;
    uiSchema?: any;
    onSubmit: (item: T) => Promise<any>;
}

export const GenericTypeCreateEditPage = <T extends unknown>(props: GenericTypeCreateEditPageProps<T>) => {

    const { [props.paramName ?? "id"]: id } = useParams();
    const [ isLoadingSchema, setIsLoadingSchema ] = useState<boolean>(true);
    const [ isLoadingItem, setIsLoadingItem ] = useState<boolean>(!!props.itemLoader);
    const [ schema, setSchema ] = useState<any>();
    const [ formData, setFormData ] = useState<T>(props.item ?? { id: uuid() } as T);
    const [ isSubmitting, setIsSubmitting ] = useState<boolean>(false);

    useEffect(() => {
        const loadSchema = buildLoadObjectFunc(
            `api/schemas/${props.typeName}`,
            {},
            resolveText("GenericTypeCreateEditPage_CoultNotLoadSchema"),
            item => {
                const translatedSchema = translateSchema(item);
                delete translatedSchema.$schema;
                setSchema(translatedSchema);
            },
            () => setIsLoadingSchema(false)
        );
        loadSchema()
    }, [ props.typeName ]);

    useEffect(() => {
        if(!props.itemLoader || !id) {
            setIsLoadingItem(false);
            return;
        }
        const loadItem = async () => {
            try {
                const item = await props.itemLoader!(id);
                setFormData(item);
            } catch(error: any) {
                NotificationManager.error(error.message, resolveText("GenericTypeCreateEditPage_CoultNotLoadItem"));
            } finally {
                setIsLoadingItem(false);
            }
        };
        loadItem();
    }, [ props.itemLoader, id ]);

    const onChange = (e: IChangeEvent) => {
        setFormData(e.formData);
    }
    
    const onSubmit = async () => {
        setIsSubmitting(true);
        try {
            await props.onSubmit(formData);
        } catch(error: any) {
            NotificationManager.error(error.message, resolveText("GenericTypeCreateEditPage_CoultNotSubmit"));
        } finally {
            setIsSubmitting(false);
        }
    }

    if(isLoadingSchema || isLoadingItem) {
        return (<h3>{resolveText("Loading...")}</h3>);
    }
    if(!schema) {
        return (<h3>{resolveText("GenericTypeCreateEditPage_CoultNotLoadSchema")}</h3>)
    }
    return (
        <Form
            schema={schema}
            formData={formData}
            onChange={onChange}
            onError={() => {}}
            onSubmit={onSubmit}
            uiSchema={Object.assign({
                id: {
                    "ui:readonly": true
                }
            }, props.uiSchema ?? {})}
            ArrayFieldTemplate={AccordionArrayFieldTemplate}
        >
            <AsyncButton
                type='submit'
                activeText={resolveText("Submit")}
                executingText={resolveText("Submitting...")}
                isExecuting={isSubmitting}
            />
        </Form>
    );

}
