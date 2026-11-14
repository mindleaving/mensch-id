using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models.ExternalServices;

public class ServiceChallengeResult
{
    [JsonProperty("challenge_id")]
    [JsonPropertyName("challenge_id")]
    public string ChallengeId { get; set; }

    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public ServiceChallengeResultStatus Status { get; set; }

    [JsonProperty("result_token")]
    [JsonPropertyName("result_token")]
    public string ResultToken { get; set; }
}
