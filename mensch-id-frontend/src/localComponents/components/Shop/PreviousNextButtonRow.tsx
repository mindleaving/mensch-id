import { Button, Col, Row } from "react-bootstrap";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { ReactNode } from "react";

interface PreviousNextButtonRowProps {
    onPrevious?: () => void;
    onNext?: () => void;
    canMoveNext?: boolean;
    nextButton?: ReactNode;
}

export const PreviousNextButtonRow = (props: PreviousNextButtonRowProps) => {

    const { onPrevious, onNext, canMoveNext, nextButton } = props;

    return (
    <Row className="mt-3">
        {onPrevious
        ? <Col xs="auto">
            <Button
                onClick={onPrevious}
            >
                &lt; {resolveText("Previous")}
            </Button>
        </Col> : null}
        <Col />
        {onNext
        ? <Col xs="auto">
            {nextButton ??
            <Button
                onClick={onNext}
                disabled={!canMoveNext}
            >
                {resolveText("Next")} &gt;
            </Button>}
        </Col> : null}
    </Row>
    );

}