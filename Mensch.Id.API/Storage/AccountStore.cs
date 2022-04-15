using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using MongoDB.Driver;

namespace Mensch.Id.API.Storage
{
    public class AccountStore : GenericStore<Account>, IAccountStore
    {
        public AccountStore(
            IMongoDatabase mongoDatabase,
            string collectionName = null)
            : base(mongoDatabase, collectionName)
        {
        }

        public Task<LocalAccount> GetLocalByIdAsync(string username)
        {
            return collection.OfType<LocalAccount>()
                .Find(x => x.Username == username)
                .FirstOrDefaultAsync();
        }

        public Task<ExternalAccount> GetExternalByIdAsync(
            LoginProvider loginProvider,
            string externalId)
        {
            return collection.OfType<ExternalAccount>()
                .Find(x => x.LoginProvider == loginProvider && x.ExternalId == externalId)
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

        public async Task<StorageResult> ChangePasswordAsync(
            string username,
            string passwordBase64)
        {
            var updateBuilder = Builders<LocalAccount>.Update;
            var result = await collection.OfType<LocalAccount>()
                .UpdateOneAsync(
                    x => x.Username == username,
                    updateBuilder.Set(x => x.PasswordHash, passwordBase64));
            if(result.IsAcknowledged && result.MatchedCount == 1)
                return StorageResult.Success();
            return StorageResult.Error(StoreErrorType.NoMatch);
        }

        public Task DeleteAllForPerson(
            string personId)
        {
            return collection.DeleteManyAsync(x => x.PersonId == personId);
        }
    }
}
