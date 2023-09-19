using System;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models
{
    public class AssignedProfilesRequestParameters
    {
        public int? Count { get; set; }
        public int Skip { get; set; } = 0;
        [TypescriptIsOptional]
        public string SearchText { get; set; }
        public DateTime? TimeRangeStart { get; set; }
        public DateTime? TimeRangeEnd { get; set; }
    }
}
