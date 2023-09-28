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
        public string PersonId { get; set; }
        public Language PreferedLanguage { get; set; } = Language.en;
    }
}
