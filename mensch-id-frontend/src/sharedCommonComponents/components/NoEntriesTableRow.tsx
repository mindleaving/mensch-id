import React from 'react';
import { resolveText } from '../helpers/Globalizer';

interface NoEntriesTableRowProps {
    colSpan: number;
}

export const NoEntriesTableRow = (props: NoEntriesTableRowProps) => {

    return (
        <tr>
            <td colSpan={props.colSpan} className='text-center'>{resolveText("NoEntries")}</td>
        </tr>
    );

}