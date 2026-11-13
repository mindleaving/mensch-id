using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models.AccessControl;

[BsonIgnoreExtraElements]
public class LocalAccount : LocalAnonymousAccount
{
    public override AccountType AccountType => AccountType.Local;
    public string Email { get; set; }
    public bool IsEmailVerified { get; set; }
    [JsonIgnore]
    [TypescriptIsOptional]
    public string EmailVerificationNonce { get; set; }
    [JsonIgnore]
    [TypescriptIsOptional]
    public string PasswordResetNonce { get; set; }
}
