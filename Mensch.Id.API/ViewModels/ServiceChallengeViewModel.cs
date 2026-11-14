using System;
using Mensch.Id.API.Models.ExternalServices;

namespace Mensch.Id.API.ViewModels;

public class ServiceChallengeViewModel
{
    public string Id { get; set; }
    public string ServiceDisplayName { get; set; }
    public string ServiceContactInformation { get; set; }
    public string Description { get; set; }
    public ServiceChallengeState State { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public DateTime ExpiresAt { get; set; }
}
