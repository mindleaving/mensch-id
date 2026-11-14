using System;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models.ExternalServices;

public class ServiceChallengeCreationResponse
{
    [JsonProperty("challenge_id")]
    public string ChallengeId { get; set; }

    [JsonProperty("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [JsonProperty("app_link")]
    public string AppLink { get; set; }
}
