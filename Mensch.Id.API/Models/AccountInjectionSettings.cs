using System.Collections.Generic;

namespace Mensch.Id.API.Models
{
    public class AccountInjectionSettings
    {
        public const string AppSettingsSectionName = "AccountInjection";

        public List<AccountInjectorCredentials> Credentials { get; set; } = new();
    }

    public class AccountInjectorCredentials
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
    }
}
