import React, { ReactNode } from 'react';
import { Col, Row } from 'react-bootstrap';

interface UniformGridProps {
    columnCount: number;
    items: ReactNode[];
    size: "xs" | "sm" | "md" | "lg" | "xl";
    className?: string;
}

export const UniformGrid = (props: UniformGridProps) => {

    const columnWidth = 12 / props.columnCount;
    const rows = [];
    let currentRowColumns = [];
    for (let index = 0; index < props.items.length; index++) {
        const element = props.items[index];
        const column = props.size === "sm" ? <Col sm={columnWidth} key={currentRowColumns.length} >{element}</Col>
            : props.size === "md" ? <Col md={columnWidth} key={currentRowColumns.length} >{element}</Col>
            : props.size === "lg" ? <Col lg={columnWidth} key={currentRowColumns.length} >{element}</Col>
            : props.size === "xl" ? <Col xl={columnWidth} key={currentRowColumns.length} >{element}</Col>
            : <Col xs={columnWidth} key={currentRowColumns.length} >{element}</Col>;
        currentRowColumns.push(column);
        if(currentRowColumns.length === props.columnCount) {
            rows.push(<Row key={rows.length} className='mb-2 g-2'>
                {currentRowColumns}
            </Row>);
            currentRowColumns = [];
        }
    }
    if(currentRowColumns.length > 0) {
        rows.push(<Row key={rows.length} className='mb-2 g-2'>
            {currentRowColumns}
        </Row>);
    }
    return (<div className={props.className}>
        {rows}
    </div>);
}