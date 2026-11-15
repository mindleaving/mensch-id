using MongoDB.Bson.Serialization.Attributes;

namespace Mensch.Id.API.Models.AccessControl;

[BsonIgnoreExtraElements]
public class ExternalAccount : PersonAccount
{
    public override AccountType AccountType => AccountType.External;
    public LoginProvider LoginProvider { get; set; }
    public string ExternalId { get; set; }
}
