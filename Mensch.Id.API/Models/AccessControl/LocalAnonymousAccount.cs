using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models.AccessControl;

[BsonIgnoreExtraElements]
public class LocalAnonymousAccount : PersonAccount, IAccountWithPassword
{
    public override AccountType AccountType => AccountType.LocalAnonymous;
    [JsonIgnore]
    [TypescriptIsOptional]
    public string PasswordHash { get; set; }
}
