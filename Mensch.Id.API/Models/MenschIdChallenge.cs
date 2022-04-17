using System;

namespace Mensch.Id.API.Models
{
    public class MenschIdChallenge : IId
    {
        public string Id { get; set; }
        public string MenschId { get; set; }
        public string ChallengeShortId { get; set; }
        public string ChallengeSecret { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }
}
