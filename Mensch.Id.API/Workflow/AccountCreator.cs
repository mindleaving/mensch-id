using System;
using System.Data;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Identity;

namespace Mensch.Id.API.Workflow
{
    public class AccountCreator : IAccountCreator
    {
        private readonly IAccountStore accountStore;
        private readonly IExternalLoginObscurer externalLoginObscurer;
        private readonly IPasswordHasher<LocalAnonymousAccount> passwordHasher;

        public AccountCreator(
            IAccountStore accountStore,
            IExternalLoginObscurer externalLoginObscurer,
            IPasswordHasher<LocalAnonymousAccount> passwordHasher)
        {
            this.accountStore = accountStore;
            this.externalLoginObscurer = externalLoginObscurer;
            this.passwordHasher = passwordHasher;
        }

        public async Task<LocalAnonymousAccount> CreateLocalAnonymous(
            string password,
            Language preferedLanguage = Language.en,
            string menschId = null)
        {
            if (menschId != null)
            {
                var existingAccount = await accountStore.FirstOrDefaultAsync(x => x is LocalAnonymousAccount && x.PersonId == menschId);
                if (existingAccount != null)
                    throw new DuplicateNameException("An account with the same email already exists");
            }
            var accountId = Guid.NewGuid().ToString();
            var account = new LocalAnonymousAccount
            {
                Id = accountId,
                PreferedLanguage = preferedLanguage,
                PersonId = menschId
            };
            account.PasswordHash = passwordHasher.HashPassword(account, password);
            await accountStore.StoreAsync(account);
            return account;
        }

        public async Task<LocalAccount> CreateLocal(
            string email,
            string password,
            Language preferedLanguage = Language.en,
            string menschId = null)
        {
            var existingAccount = await accountStore.GetLocalByEmailAsync(email);
            if (existingAccount != null)
                throw new DuplicateNameException("An account with the same email already exists");
            var accountId = Guid.NewGuid().ToString();
            var emailVerificationAndPasswordResetSalt = Convert.ToBase64String(PasswordHasher.CreateSalt());
            var account = new LocalAccount
            {
                Id = accountId,
                Email = email,
                IsEmailVerified = false,
                EmailVerificationAndPasswordResetSalt = emailVerificationAndPasswordResetSalt,
                PreferedLanguage = preferedLanguage,
                PersonId = menschId
            };
            account.PasswordHash = passwordHasher.HashPassword(account, password);
            await accountStore.StoreAsync(account);
            return account;
        }

        public async Task<ExternalAccount> CreateExternal(
            LoginProvider loginProvider,
            string externalId,
            Language preferedLanguage = Language.en,
            string menschId = null)
        {
            var existingAccount = await accountStore.GetExternalByIdAsync(loginProvider, externalId);
            if (existingAccount != null)
                throw new DuplicateNameException("An account already exists for this external login");
            var account = new ExternalAccount
            {
                Id = Guid.NewGuid().ToString(),
                LoginProvider = loginProvider,
                ExternalId = externalLoginObscurer.Obscure(externalId),
                PreferedLanguage = preferedLanguage,
                PersonId = menschId
            };
            await accountStore.StoreAsync(account);
            return account;
        }

        public async Task<LocalAccount> CreateAssigner(
            string name,
            string email,
            string password,
            Language preferedLanguage = Language.en)
        {
            var existingAccount = await accountStore.GetLocalByEmailAsync(email);
            if (existingAccount != null)
                throw new DuplicateNameException("An account with the same email already exists");
            var accountId = Guid.NewGuid().ToString();
            var emailVerificationAndPasswordResetSalt = Convert.ToBase64String(PasswordHasher.CreateSalt());
            var account = new AssignerAccount
            {
                Id = accountId,
                Name = name,
                PreferedLanguage = preferedLanguage,
                EmailVerificationAndPasswordResetSalt = emailVerificationAndPasswordResetSalt,
                Email = email,
                IsEmailVerified = false
            };
            account.PasswordHash = passwordHasher.HashPassword(account, password);
            await accountStore.StoreAsync(account);
            return account;
        }
    }
}
