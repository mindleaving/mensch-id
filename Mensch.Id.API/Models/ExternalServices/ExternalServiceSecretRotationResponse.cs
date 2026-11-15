using Newtonsoft.Json;

namespace Mensch.Id.API.Models.ExternalServices;

public class ExternalServiceSecretRotationResponse
{
    [JsonProperty("client_secret")]
    public string ClientSecret { get; set; }
}
