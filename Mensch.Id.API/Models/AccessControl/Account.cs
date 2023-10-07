using MongoDB.Bson.Serialization.Attributes;

namespace Mensch.Id.API.Models.AccessControl
{
    [BsonKnownTypes(
        typeof(ExternalAccount), 
        typeof(LocalAnonymousAccount), 
        typeof(LocalAccount),
        typeof(AssignerAccount),
        typeof(AdminAccount)
    )]
    public abstract class Account : IId
    {
        public string Id { get; set; }
        public abstract AccountType AccountType { get; }
        public Language PreferedLanguage { get; set; } = Language.en;
    }

    public abstract class PersonAccount : Account
    {
        public string PersonId { get; set; }
    }
}
