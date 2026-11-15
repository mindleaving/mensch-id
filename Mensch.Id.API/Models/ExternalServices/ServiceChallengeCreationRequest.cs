using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models.ExternalServices;

public class ServiceChallengeCreationRequest : ExternalServiceCredentials
{
    [Required]
    [JsonProperty("mensch_id")]
    public string MenschId { get; set; }

    [Required]
    [MaxLength(2000)]
    [JsonProperty("description")]
    public string Description { get; set; }

    [MaxLength(512)]
    [JsonProperty("state")]
    public string State { get; set; }
}
