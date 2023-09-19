import React from 'react';
import { resolveText } from '../helpers/Globalizer';

interface LoadingTableRowProps {
    colSpan: number;
}

export const LoadingTableRow = (props: LoadingTableRowProps) => {

    return (
        <tr>
            <td colSpan={props.colSpan} className='text-center text-secondary'>{resolveText("Loading...")}</td>
        </tr>
    );

}