import { addDays } from 'date-fns';
import { Row, Col, Button } from 'react-bootstrap';
import { DateFormControl } from './FormControls/DateFormControl';

interface DateSelectionRowProps {
    date: string;
    onChange: (date: string) => void;
}
export const DateSelectionRow = (props: DateSelectionRowProps) => {
    const { date: selectedDate, onChange } = props;
    return (<Row>
        <Col />
        <Col xs="auto">
            <Button
                onClick={() => onChange(addDays(new Date(selectedDate), -1).toISOString())}
            >
                &lt;
            </Button>
        </Col>
        <Col xs={4} md={3} lg={2}>
            <DateFormControl
                value={selectedDate}
                onChange={date => {
                    if(date) {
                        onChange(date);
                    }
                }}
            />
        </Col>
        <Col xs="auto">
            <Button
                onClick={() => onChange(addDays(new Date(selectedDate), 1).toISOString())}
            >
                &gt;
            </Button>
        </Col>
        <Col />
    </Row>);
}
export default DateSelectionRow;