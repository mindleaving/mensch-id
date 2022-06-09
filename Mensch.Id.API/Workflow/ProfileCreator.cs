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

        public async Task<LocalAnonymousAccount> CreateLocalAnonymous(
            string password,
            Language preferedLanguage = Language.en,
            string menschId = null)
        {
            var accountId = Guid.NewGuid().ToString();
            var salt = PasswordHasher.CreateSalt();
            var passwordHash = PasswordHasher.Hash(password, salt);
            var account = new LocalAnonymousAccount
            {
                Id = accountId,
                Salt = Convert.ToBase64String(salt),
                PasswordHash = Convert.ToBase64String(passwordHash),
                PreferedLanguage = preferedLanguage,
                PersonId = menschId
            };
            await accountStore.StoreAsync(account);
            return account;
        }

        public async Task<LocalAccount> CreateLocal(
            string username,
            string password,
            Language preferedLanguage = Language.en,
            string menschId = null)
        {
            var accountId = Guid.NewGuid().ToString();
            var salt = PasswordHasher.CreateSalt();
            var passwordHash = PasswordHasher.Hash(password, salt);
            var account = new LocalAccount
            {
                Id = accountId,
                Email = username,
                IsEmailVerified = false,
                Salt = Convert.ToBase64String(salt),
                PasswordHash = Convert.ToBase64String(passwordHash),
                PreferedLanguage = preferedLanguage,
                PersonId = menschId
            };
            await accountStore.StoreAsync(account);
            return account;
        }

        public async Task<ExternalAccount> CreateExternal(
            LoginProvider loginProvider,
            string externalId,
            Language preferedLanguage = Language.en,
            string menschId = null)
        {
            var account = new ExternalAccount
            {
                Id = Guid.NewGuid().ToString(),
                LoginProvider = loginProvider,
                ExternalId = externalId,
                PreferedLanguage = preferedLanguage,
                PersonId = menschId
            };
            await accountStore.StoreAsync(account);
            return account;
        }
    }
}
