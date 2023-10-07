using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
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
            var filter = filterBuilder.And(
                filterBuilder.Ne("_t", nameof(ExternalAccount)),
                filterBuilder.Eq(nameof(LocalAccount.Email), email));
            return await collection.Find(filter).FirstOrDefaultAsync() as LocalAccount;
        }

        public async Task<List<LocalAnonymousAccount>> GetLocalsByMenschId(
            string menschId)
        {
            if (menschId == null) throw new ArgumentNullException(nameof(menschId));
            var filterBuilder = Builders<Account>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Ne("_t", nameof(ExternalAccount)),
                filterBuilder.Eq(nameof(PersonAccount.PersonId), menschId));
            var localAccounts = await collection.Find(filter).ToListAsync();
            return localAccounts.Cast<LocalAnonymousAccount>().ToList();
        }

        public async Task<List<Account>> GetLocalsByEmailMenschIdOrUsernameAsync(string emailMenschIdOrUsername)
        {
            if (MenschIdGenerator.ValidateId(emailMenschIdOrUsername))
            {
                return (await GetLocalsByMenschId(emailMenschIdOrUsername)).Cast<Account>().ToList();
            }
            if (EmailValidator.IsValidEmailFormat(emailMenschIdOrUsername))
            {
                var localAccount = await GetLocalByEmailAsync(emailMenschIdOrUsername);
                if(localAccount != null)
                    return new List<Account> { localAccount };
            }

            var assignerOrAdminAccount = await GetProfessionalByUsernameAsync(emailMenschIdOrUsername);
            if(assignerOrAdminAccount != null)
                return new List<Account> { assignerOrAdminAccount };
            return new List<Account>();
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

        public async Task<List<PersonAccount>> GetAllForMenschIdAsync(
            string menschId)
        {
            if (menschId == null) throw new ArgumentNullException(nameof(menschId));
            var localAccounts = await collection.OfType<LocalAccount>().Find(x => x.PersonId == menschId).ToListAsync();
            var localAnonymousAccounts = await collection.OfType<LocalAnonymousAccount>().Find(x => x.PersonId == menschId).ToListAsync();
            var externalAccounts = await collection.OfType<ExternalAccount>().Find(x => x.PersonId == menschId).ToListAsync();
            return localAccounts.Cast<PersonAccount>().Concat(localAnonymousAccounts).Concat(externalAccounts).ToList();
        }

        public async Task<ProfessionalAccount> GetProfessionalByUsernameAsync(
            string username)
        {
            var assignerAccount = await collection.OfType<AssignerAccount>().Find(x => x.Username == username).FirstOrDefaultAsync();
            if (assignerAccount != null)
                return assignerAccount;
            var adminAccount = await collection.OfType<AdminAccount>().Find(x => x.Username == username).FirstOrDefaultAsync();
            if (adminAccount != null)
                return adminAccount;
            return null;
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
                                    updateBuilder.Set(x => x.PasswordHash, passwordHash)
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

        public async Task DeleteAllForPerson(
            string personId)
        {
            if (personId == null) throw new ArgumentNullException(nameof(personId));
            await collection.OfType<LocalAnonymousAccount>().DeleteManyAsync(x => x.PersonId == personId);
            await collection.OfType<LocalAccount>().DeleteManyAsync(x => x.PersonId == personId);
            await collection.OfType<ExternalAccount>().DeleteManyAsync(x => x.PersonId == personId);
        }
    }
}
