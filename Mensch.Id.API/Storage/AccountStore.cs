﻿using System;
using System.Collections.Generic;
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
            var filterBuilder = Builders<Account>.Filter;
            var filter = filterBuilder.Eq(nameof(LocalAccount.Email), email);
            return await collection.Find(filter).FirstOrDefaultAsync() as LocalAccount;
        }

        public async Task<LocalAnonymousAccount> GetLocalByMenschId(
            string menschId)
        {
            var filterBuilder = Builders<Account>.Filter;
            var filter = filterBuilder.Eq(nameof(Account.PersonId), menschId);
            return await collection.Find(filter).FirstOrDefaultAsync() as LocalAnonymousAccount;
        }

        public async Task<LocalAnonymousAccount> GetLocalByEmailOrMenschIdAsync(string emailOrMenschId)
        {
            if (MenschIdGenerator.ValidateId(emailOrMenschId))
            {
                return await GetLocalByMenschId(emailOrMenschId);
            }
            if (EmailValidator.IsValidEmailFormat(emailOrMenschId))
            {
                return await GetLocalByEmailAsync(emailOrMenschId);
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

        public async Task<StorageResult> ChangePasswordAsync(
            string email,
            string passwordBase64)
        {
            var updateBuilder = Builders<LocalAccount>.Update;
            var result = await collection.OfType<LocalAccount>()
                .UpdateOneAsync(
                    x => x.Email == email,
                    updateBuilder.Combine(
                        updateBuilder.Set(x => x.PasswordHash, passwordBase64),
                        updateBuilder.Set(x => x.PasswordResetToken, null)
                    ));
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
