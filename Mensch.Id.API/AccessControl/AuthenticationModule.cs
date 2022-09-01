using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Identity;

namespace Mensch.Id.API.AccessControl
{
    public class AuthenticationModule : IAuthenticationModule
    {
        private readonly IAccountStore accountStore;
        private readonly ISecurityTokenBuilder securityTokenBuilder;
        private readonly IPasswordHasher<LocalAnonymousAccount> passwordHasher;

        public AuthenticationModule(
            IAccountStore accountStore,
            ISecurityTokenBuilder securityTokenBuilder,
            IPasswordHasher<LocalAnonymousAccount> passwordHasher)
        {
            this.accountStore = accountStore;
            this.securityTokenBuilder = securityTokenBuilder;
            this.passwordHasher = passwordHasher;
        }

        public async Task<bool> ChangePasswordAsync(
            string accountId,
            string password,
            bool changePasswordOnNextLogin = false)
        {
            var matchingAccount = await accountStore.GetByIdAsync(accountId) as LocalAnonymousAccount;
            if (matchingAccount == null)
                return false;
            var passwordHash = passwordHasher.HashPassword(matchingAccount, password);
            var result = await accountStore.ChangePasswordAsync(matchingAccount.Id, passwordHash);
            return result.IsSuccess;
        }

        public async Task<AuthenticationResult> AuthenticateLocalAsync(LoginInformation loginInformation)
        {
            if(string.IsNullOrEmpty(loginInformation.Password))
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidPassword);
            var accounts = await accountStore.GetLocalsByEmailOrMenschIdAsync(loginInformation.EmailOrMenschId);
            if(accounts.Count == 0)
                return AuthenticationResult.Failed(AuthenticationErrorType.UserNotFound);
            foreach (var account in accounts)
            {
                var verificationResult = passwordHasher.VerifyHashedPassword(account, account.PasswordHash, loginInformation.Password);
                if (verificationResult == PasswordVerificationResult.Failed)
                    continue;
                if (verificationResult != PasswordVerificationResult.Success && verificationResult != PasswordVerificationResult.SuccessRehashNeeded)
                    throw new ArgumentOutOfRangeException(nameof(verificationResult), $"Invalid verification result '{verificationResult}'");
                if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    account.PasswordHash = passwordHasher.HashPassword(account, loginInformation.Password);
                    await accountStore.StoreAsync(account);
                }
                if (account is LocalAccount localAccount)
                {
                    if(!localAccount.IsEmailVerified)
                        return AuthenticationResult.Failed(AuthenticationErrorType.EmailNotVerified);
                }
                return BuildSecurityTokenForUser(account);
            }
            return AuthenticationResult.Failed(AuthenticationErrorType.InvalidPassword);
        }

        public async Task<AuthenticationResult> AuthenticateExternalAsync(List<Claim> claims)
        {
            var loginProvider = ClaimsHelpers.GetLoginProvider(claims);
            if (loginProvider == LoginProvider.Unknown)
                throw new Exception("Could not determine login provider from claims");
            if (loginProvider == LoginProvider.LocalJwt)
                throw new Exception("Cannot re-authenticate local account from claims. "
                                    + "The called authentication methods is intended for external login providers");
            var externalId = ClaimsHelpers.GetExternalId(claims);
            var account = await accountStore.GetExternalByIdAsync(loginProvider, externalId);
            var token = securityTokenBuilder.BuildForExternalLoginProvider(account);
            return AuthenticationResult.Success(token, account.AccountType);
        }

        public AuthenticationResult BuildSecurityTokenForUser(LocalAnonymousAccount account)
        {
            var token = securityTokenBuilder.BuildForLocalUser(account);
            return AuthenticationResult.Success(token, account.AccountType);
        }
    }
}
