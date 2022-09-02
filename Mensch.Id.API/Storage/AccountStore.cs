using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Workflow;
using MongoDB.Driver;

namespace Mensch.Id.API.Storage
{
    public class AccountStore : GenericStore<Account>, IAccountStore
    {
        private readonly IExternalLoginObscurer externalLoginObscurer;

        public AccountStore(
            IMongoDatabase mongoDatabase,
            IExternalLoginObscurer externalLoginObscurer,
            string collectionName = null)
            : base(mongoDatabase, collectionName)
        {
            this.externalLoginObscurer = externalLoginObscurer;
        }

        public async Task<LocalAccount> GetLocalByEmailAsync(string email)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));
            var filterBuilder = Builders<Account>.Filter;
            var filter = filterBuilder.Eq(nameof(LocalAccount.Email), email);
            return await collection.Find(filter).FirstOrDefaultAsync() as LocalAccount;
        }

        public async Task<List<LocalAnonymousAccount>> GetLocalsByMenschId(
            string menschId)
        {
            if (menschId == null) throw new ArgumentNullException(nameof(menschId));
            var filterBuilder = Builders<Account>.Filter;
            var filter = filterBuilder.Eq(nameof(Account.PersonId), menschId);
            var localAccounts = await collection.Find(filter).ToListAsync();
            return localAccounts.Cast<LocalAnonymousAccount>().ToList();
        }

        public async Task<List<LocalAnonymousAccount>> GetLocalsByEmailOrMenschIdAsync(string emailOrMenschId)
        {
            if (MenschIdGenerator.ValidateId(emailOrMenschId))
            {
                return await GetLocalsByMenschId(emailOrMenschId);
            }
            if (EmailValidator.IsValidEmailFormat(emailOrMenschId))
            {
                var localAccount = await GetLocalByEmailAsync(emailOrMenschId);
                return new List<LocalAnonymousAccount> { localAccount };
            }
            return null;
        }

        public Task<ExternalAccount> GetExternalByIdAsync(
            LoginProvider loginProvider,
            string externalId)
        {
            var obscuredExternalId = externalLoginObscurer.Obscure(externalId);
            return collection.OfType<ExternalAccount>()
                .Find(x => x.LoginProvider == loginProvider && x.ExternalId == obscuredExternalId)
                .FirstOrDefaultAsync();
        }

        public async Task<Account> GetFromClaimsAsync(List<Claim> claims)
        {
            var accountIdClaim = claims.Find(x => x.Type == ClaimTypes.NameIdentifier && x.Issuer == JwtSecurityTokenBuilder.Issuer);
            if (accountIdClaim != null)
                return await GetByIdAsync(accountIdClaim.Value);

            var loginProvider = ClaimsHelpers.GetLoginProvider(claims);
            if (loginProvider == LoginProvider.Unknown)
                return null;
            if (loginProvider == LoginProvider.LocalJwt)
                throw new Exception("This should not have happened. If login provider is LocalJwt, the accountIdClaim should exist");

            var externalId = ClaimsHelpers.GetExternalId(claims, loginProvider);
            return await GetExternalByIdAsync(loginProvider, externalId);
        }

        public Task<List<Account>> GeAllForMenschIdAsync(
            string menschId)
        {
            if (menschId == null) throw new ArgumentNullException(nameof(menschId));
            return collection.Find(x => x.PersonId == menschId).ToListAsync();
        }

        public async Task<StorageResult> ChangePasswordAsync(
            string accountId,
            string passwordHash)
        {
            if (accountId == null) throw new ArgumentNullException(nameof(accountId));
            var account = await GetByIdAsync(accountId);
            if(account == null)
                return StorageResult.Error(StoreErrorType.NoMatch);
            UpdateResult result;
            switch (account.AccountType)
            {
                case AccountType.Local:
                    {
                        var updateBuilder = Builders<LocalAccount>.Update;
                        result = await collection
                            .OfType<LocalAccount>()
                            .UpdateOneAsync(
                                x => x.Id == accountId,
                                updateBuilder.Combine(
                                    updateBuilder.Set(x => x.PasswordHash, passwordHash),
                                    updateBuilder.Set(x => x.PasswordResetToken, null)
                                )
                            );
                    }
                    break;
                case AccountType.LocalAnonymous:
                    {
                        var updateBuilder = Builders<LocalAnonymousAccount>.Update;
                        result = await collection
                            .OfType<LocalAnonymousAccount>()
                            .UpdateOneAsync(
                                x => x.Id == accountId,
                                updateBuilder.Set(x => x.PasswordHash, passwordHash)
                            );
                        break;
                    }
                case AccountType.External:
                    throw new InvalidOperationException("Cannot change password for external account");
                case AccountType.Assigner:
                    {
                        var updateBuilder = Builders<AssignerAccount>.Update;
                        result = await collection
                            .OfType<AssignerAccount>()
                            .UpdateOneAsync(
                                x => x.Id == accountId,
                                updateBuilder.Combine(
                                    updateBuilder.Set(x => x.PasswordHash, passwordHash),
                                    updateBuilder.Set(x => x.PasswordResetToken, null)
                                )
                            );
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (result.IsAcknowledged && result.MatchedCount == 1)
                return StorageResult.Success();
            return StorageResult.Error(StoreErrorType.NoMatch);
        }

        public Task DeleteAllForPerson(
            string personId)
        {
            if (personId == null) throw new ArgumentNullException(nameof(personId));
            return collection.DeleteManyAsync(x => x.PersonId == personId);
        }
    }
}
