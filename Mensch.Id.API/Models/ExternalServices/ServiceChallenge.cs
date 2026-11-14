using System;

namespace Mensch.Id.API.Models.ExternalServices;

public class ServiceChallenge : IId
{
    public string Id { get; set; }
    public Guid ServiceId { get; set; }
    public string MenschId { get; set; }
    public string Description { get; set; }
    public string ServiceState { get; set; }
    public ServiceChallengeState State { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AnsweredTimestamp { get; set; }
    public string ResultToken { get; set; }
}
