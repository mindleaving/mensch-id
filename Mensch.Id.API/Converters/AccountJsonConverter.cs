using System;
using Mensch.Id.API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mensch.Id.API.Converters
{
    public class AccountJsonConverter : JsonConverter<Account>
    {
        public override bool CanWrite => false;

        public override void WriteJson(
            JsonWriter writer,
            Account value,
            JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override Account ReadJson(
            JsonReader reader,
            Type objectType,
            Account existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            var accountTypeToken = jObject
                .GetValue(nameof(Account.AccountType), StringComparison.InvariantCultureIgnoreCase)?
                .Value<string>();
            var accountType = Enum.Parse<AccountType>(accountTypeToken, true);
            Account account;
            switch (accountType)
            {
                case AccountType.Local:
                    account = new LocalAccount();
                    break;
                case AccountType.LocalAnonymous:
                    account = new LocalAnonymousAccount();
                    break;
                case AccountType.External:
                    account = new ExternalAccount();
                    break;
                case AccountType.Assigner:
                    account = new AssignerAccount();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            serializer.Populate(jObject.CreateReader(), account);
            return account;
        }
    }
}
