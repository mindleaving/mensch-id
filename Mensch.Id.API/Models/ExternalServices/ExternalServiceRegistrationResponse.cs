using System;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models.ExternalServices;

public class ExternalServiceRegistrationResponse : ExternalServiceCredentials
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
}
