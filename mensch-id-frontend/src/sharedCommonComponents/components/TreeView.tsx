import React, { useState } from 'react';
import Form from 'react-bootstrap/Form';
import { Update } from '../types/frontendTypes';

export interface ITreeViewItem {
    isSelected?: boolean;
    isInitiallyExpanded?: boolean;
    subEntries: ITreeViewItem[];
}

const setSelectionForSubEntries = <T extends ITreeViewItem>(subEntries: T[], isSelected: boolean): T[] => {
    return subEntries.map(subEntry => ({
        ...subEntry,
        isSelected: isSelected,
        subEntries: setSelectionForSubEntries(subEntry.subEntries as T[], isSelected)
    }));
};
const buildIsSelectedUpdate = <T extends ITreeViewItem>(isSelected: boolean) => 
    (item: T) => ({
        ...item,
        isSelected: isSelected,
        subEntries: setSelectionForSubEntries(item.subEntries as T[], isSelected)
    });

interface TreeViewProps<T> {
    items: T[];
    displayFunc: (item: T) => string;
    selectable: boolean;
    onSelectionChanged: (update: Update<T[]>) => void;
}

export const unselectAll = (items: ITreeViewItem[]): ITreeViewItem[] => {
    return items.map(treeViewItem => ({
        ...treeViewItem,
        isSelected: false,
        subEntries: unselectAll(treeViewItem.subEntries)
    }));
}

export const TreeView = <T extends ITreeViewItem>(props: TreeViewProps<T>) => {

    const onSelectionChanged = (changedItem: T, update: Update<T>) => {
        const listUpdate: Update<T[]> = items => items.map(item => {
            if(item === changedItem){
                return update(item);
            }
            return item;
        });
        props.onSelectionChanged(listUpdate);
    }

    return (
        <ul className="treeview">
            {props.items.map((item,idx) => (
                <TreeViewItem<T>
                    key={idx + ''}
                    item={item}
                    displayFunc={props.displayFunc}
                    onSelectionChanged={update => onSelectionChanged(item, update)}
                    selectable={props.selectable}
                />
            ))}
        </ul>
    );
}

interface TreeViewItemProps<T> {
    item: T;
    displayFunc: (item: T) => string;
    selectable: boolean;
    onSelectionChanged: (update: Update<T>) => void;
}

const TreeViewItem = <T extends ITreeViewItem>(props: TreeViewItemProps<T>) => {
    const [ isExpanded, setIsExpanded ] = useState<boolean>(props.item.isInitiallyExpanded ?? false);

    const setIsSelected = (isSelected: boolean) => {
        props.onSelectionChanged(buildIsSelectedUpdate(isSelected));
    }
    const onSubEntrySelectionChanged = (listUpdate: Update<T[]>) => {
        const subEntryUpdate: Update<T> = (item: T) => ({
            ...item,
            subEntries: listUpdate(item.subEntries as T[])
        });
        props.onSelectionChanged(subEntryUpdate);
    }
    
    const hasSubItems = props.item.subEntries.length > 0;
    return (
        <li>
            {props.selectable
            ? <Form.Check inline
                checked={props.item.isSelected ?? false}
                onChange={(e:any) => setIsSelected(e.target.checked)}
            /> : null}
            {hasSubItems 
            ? <b className="expand-icon" onClick={() => setIsExpanded(!isExpanded)}>{isExpanded ? '-' : '+'} </b>
            : null}
            <span>{props.displayFunc(props.item)}</span>
            {hasSubItems && isExpanded ?
            <TreeView
                items={props.item.subEntries as T[]}
                displayFunc={props.displayFunc}
                selectable={props.selectable}
                onSelectionChanged={onSubEntrySelectionChanged}
            /> : null}
        </li>
    );
}