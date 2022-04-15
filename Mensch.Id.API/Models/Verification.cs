using System;

namespace Mensch.Id.API.Models
{
    public class Verification : IId
    {
        public string Id { get; set; }
        public string PersonId { get; set; }
        public string VerifierId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}