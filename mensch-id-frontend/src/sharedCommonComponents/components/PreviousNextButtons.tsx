import { Row, Col } from 'react-bootstrap';
import { resolveText } from '../helpers/Globalizer';
import { useState } from 'react';
import { AsyncButton } from './AsyncButton';

interface PreviousNextButtonsProps {
    canGoPrevious?: boolean;
    onPrevious?: () => void;
    canGoNext?: boolean;
    onNext?: () => Promise<void>;
    nextText?: string;
    nextButtonSize?: 'sm' | 'lg';
}

export const PreviousNextButtons = (props: PreviousNextButtonsProps) => {

    const { canGoPrevious, onPrevious, canGoNext, onNext, nextText, nextButtonSize } = props;

    const [ isSwitchingStep, setIsSwitchingStep ] = useState<boolean>(false);

    const movePrevious = async () => {
        if(!onPrevious) {
            return;
        }
        setIsSwitchingStep(true);
        try {
            await onPrevious();
        } finally {
            setIsSwitchingStep(false);
        }
    }

    const moveNext = async () => {
        if(!onNext) {
            return;
        }
        setIsSwitchingStep(true);
        try {
            await onNext();
        } finally {
            setIsSwitchingStep(false);
        }
    }

    return (
    <Row className='mt-3'>
        <Col xs="auto">
            <AsyncButton 
                onClick={movePrevious}
                disabled={!canGoPrevious}
                isExecuting={isSwitchingStep}
                activeText={resolveText("Previous")}
                executingText={resolveText("Previous")}
            />
        </Col>
        <Col></Col>
        <Col xs="auto">
            <AsyncButton 
                onClick={moveNext} 
                disabled={!canGoNext}
                isExecuting={isSwitchingStep}
                activeText={nextText ?? resolveText("Next")}
                executingText={nextText ?? resolveText("Next")}
                size={nextButtonSize}
            />
        </Col>
    </Row>
    );

}