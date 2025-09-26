import { useCallback, useMemo } from 'react';
import Flatpickr, { OptionsType } from 'react-flatpickr';
import { resolveText } from '../../helpers/Globalizer';
import { isValidDate } from '../../helpers/DateHelpers';
import { format } from 'date-fns';
import { combineCssClasses } from '../../helpers/StylingHelpers';

interface DateRangeFormControlProps {
    value?: string[];
    onChange: (startDate: string | undefined, endDate: string | undefined) => void;
    disabled?: boolean;
    required?: boolean;
    enableTime?: boolean;
    noCalendar?: boolean;
    triggerOnChangeForUndefinedRange?: boolean;
    static?: boolean;
    size?: 'sm' | 'lg';
}

export const DateRangeFormControl = (props: DateRangeFormControlProps) => {

    const {
        value,
        onChange: onChangeFromProps,
        disabled,
        required,
        enableTime,
        noCalendar,
        triggerOnChangeForUndefinedRange,
        size
    } = props;

    const flatpickrOptions = useMemo(() => ({
        allowInput: true,
        noCalendar: noCalendar,
        enableTime: enableTime,
        time_24hr: true,
        mode: 'range',
        static: props.static,
        locale: {
            firstDayOfWeek: 1,
            rangeSeparator: ` ${resolveText("Date_Until")} `
        }
    } as OptionsType), [ noCalendar, enableTime, props.static ]);

    const parsedValue = useMemo(() => {
        if(!value) {
            return undefined;
        }
        if(value.length != 2) {
            return undefined;
        }
        const startDate = new Date(value[0]);
        if(!isValidDate(startDate)) {
            return undefined;
        }
        const endDate = new Date(value[1]);
        if(!isValidDate(endDate)) {
            return undefined;
        }
        return [ 
            enableTime ? format(startDate, 'yyyy-MM-dd HH:mm') : format(startDate, 'yyyy-MM-dd'),
            enableTime ? format(endDate, 'yyyy-MM-dd HH:mm') : format(endDate, 'yyyy-MM-dd')
        ]
    }, [ value, enableTime ]);

    const onChange = useCallback((dates: Date[]) => {
        if(dates.length === 2 && isValidDate(dates[0]) && isValidDate(dates[1])) { 
            onChangeFromProps(dates[0].toISOString(), dates[1].toISOString()); 
        } else if(triggerOnChangeForUndefinedRange) {
            onChangeFromProps(undefined, undefined);
        }
    }, [ onChangeFromProps, triggerOnChangeForUndefinedRange ]);

    return (
        <Flatpickr
            options={flatpickrOptions}
            className={combineCssClasses([
                "form-control",
                size ? `form-control-${size}` : undefined
            ])}
            required={required}
            disabled={disabled}
            value={parsedValue ?? ''}
            onChange={onChange} 
        />
    );

}