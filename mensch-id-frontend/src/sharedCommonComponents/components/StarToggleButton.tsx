import '../styles/star-button.css';

import { useMemo } from "react";

interface StarToggleButtonProps {
    isActive: boolean;
    onClick?: () => void;
    title?: string;
    className?: string;
}

export const StarToggleButton = (props: StarToggleButtonProps) => {

    const { isActive, onClick, title } = props;

    const icon = useMemo(() => isActive ? 'fa-star' : 'fa-star-o', [ isActive ]);

    return (<div
        onClick={onClick}
        className={`star-button ${isActive ? 'text-warning' : 'text-secondary'} ${props.className}`}
        title={title}
    >
        <i className={`fa ${icon}`} />
    </div>);

}