import React, { ReactNode } from 'react';
import { Button, Col, Row, Table } from 'react-bootstrap';
import { LoadingTableRow } from './LoadingTableRow';
import { NoEntriesTableRow } from './NoEntriesTableRow';
import { resolveText } from '../helpers/Globalizer';
import { InfiniteScroll } from './InfiniteScroll';
import { Models } from '../../localComponents/types/models';
import { Center } from './Center';
import { AsyncButton } from './AsyncButton';
import { NoMoreEntriesAlert } from './NoMoreEntriesAlert';

interface NonExhaustiveTableProps<ItemType> {
    autoScroll?: boolean;
    hover?: boolean;
    size?: 'sm';
    isLoading: boolean;
    items: ItemType[];
    hasMoreItems: boolean;
    loadMoreItems: () => void;
    tableHeaderBuilder?: () => ReactNode;
    tableRowBuilder: (item: ItemType) => ReactNode;
    colSpan: number;
    hasCreateNewButton?: boolean;
    onCreateNew?: () => void;
    additionalButtons?: ReactNode[];
    showNoMoreItemsAlert?: boolean;
    title?: ReactNode;
}

export const NonExhaustiveTable = <ItemType extends Models.IId>(props: NonExhaustiveTableProps<ItemType>) => {

    const { 
        autoScroll, 
        hover, 
        size,
        isLoading, 
        items,
        hasMoreItems,
        loadMoreItems,
        hasCreateNewButton,
        onCreateNew,
        additionalButtons,
        tableHeaderBuilder,
        tableRowBuilder,
        colSpan,
        showNoMoreItemsAlert,
        title
    } = props;

    const table = (
        <Table hover={hover} size={size}>
            {tableHeaderBuilder ? tableHeaderBuilder() : null}
            <tbody>
                {items.length > 0
                ? items.map(tableRowBuilder)
                : isLoading ? <LoadingTableRow colSpan={colSpan} />
                : <NoEntriesTableRow colSpan={colSpan} />}
            </tbody>
        </Table>
    )
    return (
    <>
        <Row className='align-items-center'>
            <Col>{title}</Col>
            {additionalButtons
            ? additionalButtons.map((additionalButton,buttonIndex) => (
                <Col key={buttonIndex} xs="auto">
                    {additionalButton}
                </Col>)) 
            : null}
            {hasCreateNewButton 
            ? <Col xs="auto">
                <Button
                    onClick={onCreateNew}
                >
                    {resolveText('CreateNew')}
                </Button>
            </Col> : null}
        </Row>
        {autoScroll
        ? <InfiniteScroll
            dataLength={items.length}
            hasMore={hasMoreItems}
            next={loadMoreItems}
            isLoading={isLoading}
            showNoMoreItemsAlert={showNoMoreItemsAlert}
        >
            {table}
        </InfiniteScroll>
        : <>
            {table}
            {hasMoreItems
            ? <Center>
                <AsyncButton
                    variant='link'
                    onClick={loadMoreItems}
                    activeText={resolveText("LoadMore")}
                    executingText={resolveText("Loading...")}
                    isExecuting={isLoading}
                />
            </Center> 
            : showNoMoreItemsAlert ? <NoMoreEntriesAlert />
            : null}
        </>}
    </>
    );

}