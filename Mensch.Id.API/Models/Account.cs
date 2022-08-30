using MongoDB.Bson.Serialization.Attributes;

namespace Mensch.Id.API.Models
{
    [BsonKnownTypes(
        typeof(ExternalAccount), 
        typeof(LocalAnonymousAccount), 
        typeof(LocalAccount),
        typeof(AssignerAccount)
    )]
    public abstract class Account : IId
    {
        public string Id { get; set; }
        public abstract AccountType AccountType { get; }
        public string PersonId { get; set; }
        public Language PreferedLanguage { get; set; } = Language.en;
    }

    public class LocalAnonymousAccount : Account
    {
        public override AccountType AccountType => AccountType.LocalAnonymous;
        public string Salt { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordResetToken { get; set; }
    }

    public class LocalAccount : LocalAnonymousAccount
    {
        public override AccountType AccountType => AccountType.Local;
        public string Email { get; set; }
        public string EmailVerificationToken { get; set; }
        public bool IsEmailVerified { get; set; }
    }

    public class ExternalAccount : Account
    {
        public override AccountType AccountType => AccountType.External;
        public LoginProvider LoginProvider { get; set; }
        public string ExternalId { get; set; }
    }
}
