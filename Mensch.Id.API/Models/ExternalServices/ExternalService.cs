using System;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models.ExternalServices;

[BsonIgnoreExtraElements]
public class ExternalService : IId<Guid>
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public bool IsEmailVerified { get; set; }
    [JsonIgnore]
    [TypescriptIsOptional]
    public string EmailVerificationNonce { get; set; }
    public string ContactInformation { get; set; }
    public ExternalServiceStatus Status { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public DateTime? ActivatedTimestamp { get; set; }
    public string ActivatedBy { get; set; }
}
