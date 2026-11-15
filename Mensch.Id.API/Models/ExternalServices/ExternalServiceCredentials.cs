using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models.ExternalServices;

public class ExternalServiceCredentials
{
    [Required]
    [JsonProperty("client_id")]
    public Guid ClientId { get; set; }

    [Required]
    [MaxLength(200)]
    [JsonProperty("client_secret")]
    public string ClientSecret { get; set; }
}
