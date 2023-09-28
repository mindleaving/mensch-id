import { Col, FormGroup, FormLabel, Row } from "react-bootstrap";
import { AccordionCard } from "../../../sharedCommonComponents/components/AccordionCard";
import { DateRangeFormControl } from "../../../sharedCommonComponents/components/FormControls/DateRangeFormControl";
import { RowFormGroup } from "../../../sharedCommonComponents/components/FormControls/RowFormGroup";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { Models } from "../../types/models";

interface AssignedProfilesFilterViewProps {
    filter: Models.RequestParameters.AssignedProfilesRequestParameters;
    onChange: (update: Update<Models.RequestParameters.AssignedProfilesRequestParameters>) => void;
}

export const AssignedProfilesFilterView = (props: AssignedProfilesFilterViewProps) => {

    const { filter, onChange } = props;

    return (
    <AccordionCard standalone
        title={resolveText("Filters")}
        eventKey="assigned-profiles-filters"
    >
        <RowFormGroup
            label={resolveText("Search")}
            value={filter?.searchText ?? ''}
            onChange={searchText => onChange(state => ({
                ...state,
                searchText: searchText
            }))}
        />
        <FormGroup as={Row}>
            <FormLabel column>{resolveText("TimeRange")}</FormLabel>
            <Col>
                <DateRangeFormControl
                    value={filter?.timeRangeStart && filter?.timeRangeEnd 
                        ? [ filter.timeRangeStart, filter.timeRangeEnd ]
                        : undefined}
                    onChange={(startDate, endDate) => {
                        if(startDate && endDate) {
                            onChange(state => ({
                                ...state,
                                timeRangeStart: startDate,
                                timeRangeEnd: endDate
                            }));
                        } else {
                            onChange(state => ({
                                ...state,
                                timeRangeStart: undefined,
                                timeRangeEnd: undefined
                            }));
                        }
                    }}
                    triggerOnChangeForUndefinedRange
                />
            </Col>
        </FormGroup>
    </AccordionCard>);

}