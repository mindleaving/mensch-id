using System;

namespace Mensch.Id.API.Models.RequestParameters
{
    public class AssignedProfilesRequestParameters : GenericItemsRequestParameters
    {
        public DateTime? TimeRangeStart { get; set; }
        public DateTime? TimeRangeEnd { get; set; }
    }
}
