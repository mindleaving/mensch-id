import React, { ReactNode } from 'react';
import Badge from 'react-bootstrap/Badge';

interface ListFormControlProps<T> {
    items: T[];
    idFunc: (item: T) => string;
    displayFunc: (item: T) => string | ReactNode;
    removeItem: (item: T) => void;
}

export const ListFormControl = <T extends unknown>(props: ListFormControlProps<T>) => {
    return (
        <div className="listFormControl">
            {props.items.map(item => 
                <Badge className="itemListBadge" bg="primary" key={props.idFunc(item)}>
                    {props.displayFunc(item)} <Badge pill bg="danger" onClick={() => props.removeItem(item)}>X</Badge> 
                </Badge>
            )}
        </div>
    );
}