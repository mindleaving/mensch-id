import React, { PropsWithChildren, useEffect, useState } from 'react';
import Table from 'react-bootstrap/Table';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';
import { resolveText } from '../helpers/Globalizer';
import { OrderDirection } from '../types/enums.d';

interface PagedTableProps {
    onPageChanged: (pageIndex: number, entriesPerPage: number, orderBy?: string, orderDirection?: OrderDirection) => Promise<void>;
    orderBy?: string;
    orderDirection?: OrderDirection;
    hasCreateNewButton?: boolean;
    onCreateNew?: () => void;
    bordered?: boolean;
    enableHighlighting?: boolean;
    className?: string;
}

export const PagedTable = (props: PropsWithChildren<PagedTableProps>) => {

    const [ isLoading, setIsLoading ] = useState<boolean>(true);
    const [ entriesPerPage, setEntriesPerPage ] = useState<number>(10);
    const [ pageIndex, setPageIndex ] = useState<number>(0);

    const onPageChanged = props.onPageChanged;
    const orderBy = props.orderBy;
    const orderDirection = props.orderDirection;
    useEffect(() => {
        setPageIndex(0);
    }, [ onPageChanged, entriesPerPage ]);
    useEffect(() => {
        const loadItems = async () => {
            setIsLoading(true);
            try {
                await onPageChanged(pageIndex, entriesPerPage, orderBy, orderDirection);
            } finally {
                setIsLoading(false);
            }
        }
        loadItems();
    }, [ onPageChanged, pageIndex, entriesPerPage, orderBy, orderDirection ]);

    const gotoPreviousPage = () => {
        if(pageIndex === 0) return;
        setPageIndex(pageIndex-1);
    }
    const gotoNextPage = () => {
        setPageIndex(pageIndex+1);
    }

    const navigationButtons = (<>
        <Button className="m-1" onClick={() => setPageIndex(0)} disabled={pageIndex === 0}><i className="fa fa-step-backward" /></Button>
        <Button className="m-1" onClick={gotoPreviousPage} disabled={pageIndex === 0}>{resolveText('Previous')}</Button>
        <Button className="m-1" onClick={gotoNextPage}>{resolveText('Next')}</Button>
    </>);
    return (
        <>
        <Row>
            <Col xs="auto">
                {navigationButtons}
            </Col>
            <Form.Label column xs="auto">Entries per page</Form.Label>
            <Col xs="auto">
                <Form.Control
                    as="select"
                    value={entriesPerPage}
                    onChange={(e:any) => setEntriesPerPage(e.target.value)}
                >
                    <option value="5">5</option>
                    <option value="10">10</option>
                    <option value="20">20</option>
                    <option value="30">30</option>
                    <option value="50">50</option>
                    <option value="100">100</option>
                </Form.Control>
            </Col>
            <Col></Col>
            <Col xs="auto">
                {props.hasCreateNewButton ? <Button className="m-1 float-right" onClick={props.onCreateNew}>{resolveText('CreateNew')}</Button> : null}
            </Col>
        </Row>
        <Table bordered={props.bordered} className={props.className} hover={props.enableHighlighting}>
            {isLoading 
            ? <tbody>
                <tr>
                    <td className="text-center">Loading...</td>
                </tr>
            </tbody>
            : props.children}
        </Table>
        <Row>
            <Col>
                {navigationButtons}
            </Col>
        </Row>
        </>
    );
}