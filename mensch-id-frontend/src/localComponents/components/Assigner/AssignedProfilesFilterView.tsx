import { Col, FormGroup, FormLabel, Row } from "react-bootstrap";
import { AccordionCard } from "../../../sharedCommonComponents/components/AccordionCard";
import { DateRangeFormControl } from "../../../sharedCommonComponents/components/FormControls/DateRangeFormControl";
import { RowFormGroup } from "../../../sharedCommonComponents/components/FormControls/RowFormGroup";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { Models } from "../../types/models";
import { endOfDay } from "date-fns";

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
        <FormGroup as={Row} className="my-1">
            <FormLabel column>{resolveText("AssignerControlledProfile_CreationDate")}</FormLabel>
            <Col>
                <DateRangeFormControl
                    value={filter?.creationTimeRangeStart && filter?.creationTimeRangeEnd 
                        ? [ filter.creationTimeRangeStart, filter.creationTimeRangeEnd ]
                        : undefined}
                    onChange={(startDate, endDate) => {
                        if(startDate && endDate) {
                            onChange(state => ({
                                ...state,
                                creationTimeRangeStart: startDate,
                                creationTimeRangeEnd: endOfDay(new Date(endDate)).toISOString() as any
                            }));
                        } else {
                            onChange(state => ({
                                ...state,
                                creationTimeRangeStart: undefined,
                                creationTimeRangeEnd: undefined
                            }));
                        }
                    }}
                    triggerOnChangeForUndefinedRange
                />
            </Col>
        </FormGroup>
        <FormGroup as={Row} className="my-1">
            <FormLabel column>{resolveText("BirthDate")}</FormLabel>
            <Col>
                <DateRangeFormControl
                    value={filter?.birthDateTimeRangeStart && filter?.birthDateTimeRangeEnd 
                        ? [ filter.birthDateTimeRangeStart, filter.birthDateTimeRangeEnd ]
                        : undefined}
                    onChange={(startDate, endDate) => {
                        if(startDate && endDate) {
                            onChange(state => ({
                                ...state,
                                birthDateTimeRangeStart: startDate,
                                birthDateTimeRangeEnd: endOfDay(new Date(endDate)).toISOString() as any
                            }));
                        } else {
                            onChange(state => ({
                                ...state,
                                birthDateTimeRangeStart: undefined,
                                birthDateTimeRangeEnd: undefined
                            }));
                        }
                    }}
                    triggerOnChangeForUndefinedRange
                />
            </Col>
        </FormGroup>
    </AccordionCard>);

}