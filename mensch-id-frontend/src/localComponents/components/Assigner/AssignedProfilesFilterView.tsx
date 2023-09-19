import { AccordionCard } from "../../../sharedCommonComponents/components/AccordionCard";
import { RowFormGroup } from "../../../sharedCommonComponents/components/FormControls/RowFormGroup";
import { resolveText } from "../../../sharedCommonComponents/helpers/Globalizer";
import { Update } from "../../../sharedCommonComponents/types/frontendTypes";
import { AssignedProfilesFilter } from "../../types/frontendTypes";

interface AssignedProfilesFilterViewProps {
    filter: AssignedProfilesFilter;
    onChange: (update: Update<AssignedProfilesFilter>) => void;
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
    </AccordionCard>);

}