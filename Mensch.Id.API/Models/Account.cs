using Mensch.Id.API.Converters;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models
{
    [BsonKnownTypes(
        typeof(ExternalAccount), 
        typeof(LocalAnonymousAccount), 
        typeof(LocalAccount),
        typeof(AssignerAccount)
    )]
    [JsonConverter(typeof(AccountJsonConverter))]
    public abstract class Account : IId
    {
        public string Id { get; set; }
        public abstract AccountType AccountType { get; }
        public string PersonId { get; set; }
        public Language PreferedLanguage { get; set; } = Language.en;
    }
}
