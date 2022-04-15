using System;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;

namespace Mensch.Id.API.Workflow
{
    public class ProfileCreator : IProfileCreator
    {
        private readonly IStore<Account> accountStore;

        public ProfileCreator(
            IStore<Account> accountStore)
        {
            this.accountStore = accountStore;
        }

        public async Task<LocalAccount> CreateLocal(
            string username,
            string password,
            Language preferedLanguage = Language.en,
            string personId = null)
        {
            var salt = PasswordHasher.CreateSalt();
            var passwordHash = PasswordHasher.Hash(password, salt, PasswordHasher.RecommendedHashLength);
            var account = new LocalAccount
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                Salt = Convert.ToBase64String(salt),
                PasswordHash = Convert.ToBase64String(passwordHash),
                PreferedLanguage = preferedLanguage,
                PersonId = personId
            };
            await accountStore.StoreAsync(account);
            return account;
        }

        public async Task<ExternalAccount> CreateExternal(
            LoginProvider loginProvider,
            string externalId,
            Language preferedLanguage = Language.en,
            string personId = null)
        {
            var account = new ExternalAccount
            {
                Id = Guid.NewGuid().ToString(),
                LoginProvider = loginProvider,
                ExternalId = externalId,
                PreferedLanguage = preferedLanguage,
                PersonId = personId
            };
            await accountStore.StoreAsync(account);
            return account;
        }
    }
}
