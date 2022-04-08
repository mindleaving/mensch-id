import { ArrayFieldTemplateProps } from "@rjsf/core"
import { Accordion, Button } from "react-bootstrap"
import { resolveText } from "../helpers/Globalizer"

export const AccordionArrayFieldTemplate = (props: ArrayFieldTemplateProps) => {
    return (
    <>
        {props.TitleField(props as any)}
        {props.DescriptionField(props as any)}
        <Accordion className="accordion-card" key={props.formData.id}>
            {props.items.map((item,index) => (
                <Accordion.Item key={index} eventKey={index + ""}>
                    <Accordion.Header>
                        {resolveText("ItemX").replace("{0}", index + '')}
                    </Accordion.Header>
                    <Accordion.Body>
                        {item.children}
                        <Button variant="danger" className="mt-2" onClick={() => item.onDropIndexClick(index)()}>{resolveText("Delete")}</Button>
                    </Accordion.Body>
                </Accordion.Item>
            ))}
        </Accordion>
        {props.canAdd ? <Button className='m-2' size="sm" onClick={props.onAddClick}>{resolveText("Add")}</Button> : null}
    </>
    )
}