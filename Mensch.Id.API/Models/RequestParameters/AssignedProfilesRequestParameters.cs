using System;

namespace Mensch.Id.API.Models.RequestParameters
{
    public class AssignedProfilesRequestParameters : GenericItemsRequestParameters
    {
        public DateTime? CreationTimeRangeStart { get; set; }
        public DateTime? CreationTimeRangeEnd { get; set; }

        public DateTime? BirthDateTimeRangeStart { get; set; }
        public DateTime? BirthDateTimeRangeEnd { get; set; }
    }
}
