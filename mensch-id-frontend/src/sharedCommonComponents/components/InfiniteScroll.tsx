import { PropsWithChildren } from 'react';
import InfiniteScrollFromLibrary from 'react-infinite-scroll-component';
import { LoadingAlert } from './LoadingAlert';
import { NoMoreEntriesAlert } from './NoMoreEntriesAlert';
import { NoEntriesAlert } from './NoEntriesAlert';

interface InfiniteScrollProps {
    dataLength: number;
    isLoading: boolean;
    hasMore: boolean;
    next: () => void;
    scrollThreshold?: string | number;
    className?: string;
    showNoMoreItemsAlert?: boolean;
}

export const InfiniteScroll = (props: PropsWithChildren<InfiniteScrollProps>) => {

    const { 
        dataLength,
        isLoading,
        hasMore,
        next,
        scrollThreshold,
        className,
        showNoMoreItemsAlert 
    } = props;

    return (
        <InfiniteScrollFromLibrary
            dataLength={dataLength}
            hasMore={hasMore}
            next={next}
            scrollThreshold={scrollThreshold}
            loader={<LoadingAlert variant='secondary' />}
            endMessage={dataLength > 0 && showNoMoreItemsAlert ? <NoMoreEntriesAlert /> : null}
            className={className}
            style={{ overflowX: 'hidden' }}
        >
            {dataLength > 0
            ? props.children
            : isLoading ? <LoadingAlert />
            : <NoEntriesAlert />}
        </InfiniteScrollFromLibrary>
    );

}