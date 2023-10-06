using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Identity;

namespace Mensch.Id.API.AccessControl
{
    public class AuthenticationModule : IAuthenticationModule
    {
        private readonly IAccountStore accountStore;
        private readonly ISecurityTokenBuilder securityTokenBuilder;
        private readonly IPasswordHasher<LocalAnonymousAccount> passwordHasher;
        private IClaimBuilder claimsBuilder;

        public AuthenticationModule(
            IAccountStore accountStore,
            ISecurityTokenBuilder securityTokenBuilder,
            IPasswordHasher<LocalAnonymousAccount> passwordHasher,
            IClaimBuilder claimsBuilder)
        {
            this.accountStore = accountStore;
            this.securityTokenBuilder = securityTokenBuilder;
            this.passwordHasher = passwordHasher;
            this.claimsBuilder = claimsBuilder;
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

        public async Task<AuthenticationResult> AuthenticateLocalByEmailMenschIdOrUsernameAsync(LoginInformation loginInformation)
        {
            if(string.IsNullOrEmpty(loginInformation.Password))
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
            var accounts = await accountStore.GetLocalsByEmailMenschIdOrUsernameAsync(loginInformation.EmailMenschIdOrUsername);
            if(accounts.Count == 0)
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
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
            return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
        }

        public async Task<AuthenticationResult> AuthenticateLocalByAccountIdAsync(
            string accountId,
            string password)
        {
            if(string.IsNullOrEmpty(password))
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
            var account = await accountStore.GetByIdAsync(accountId) as LocalAnonymousAccount;
            if(account == null)
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
            var verificationResult = passwordHasher.VerifyHashedPassword(account, account.PasswordHash, password);
            switch (verificationResult)
            {
                case PasswordVerificationResult.Failed:
                    return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
                case PasswordVerificationResult.SuccessRehashNeeded:
                    account.PasswordHash = passwordHasher.HashPassword(account, password);
                    await accountStore.StoreAsync(account);
                    break;
                case PasswordVerificationResult.Success:
                    // No additional actions needed
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(verificationResult), $"Invalid verification result '{verificationResult}'");
            }

            if (account is LocalAccount localAccount)
            {
                if(!localAccount.IsEmailVerified)
                    return AuthenticationResult.Failed(AuthenticationErrorType.EmailNotVerified);
            }
            return BuildSecurityTokenForUser(account);
        }

        public async Task<AuthenticationResult> AuthenticateExternalAsync(List<Claim> externalClaims)
        {
            var loginProvider = ClaimsHelpers.GetLoginProvider(externalClaims);
            if (loginProvider == LoginProvider.Unknown)
                throw new Exception("Could not determine login provider from claims");
            if (loginProvider == LoginProvider.LocalJwt)
                throw new Exception("Cannot re-authenticate local account from claims. "
                                    + "The called authentication methods is intended for external login providers");
            var externalId = ClaimsHelpers.GetExternalId(externalClaims);
            var account = await accountStore.GetExternalByIdAsync(loginProvider, externalId);
            var menschIdClaims = claimsBuilder.BuildForExternalLoginProvider(account);
            var token = securityTokenBuilder.Build(menschIdClaims);
            return AuthenticationResult.Success(menschIdClaims, token, account.AccountType);
        }

        public AuthenticationResult BuildSecurityTokenForUser(LocalAnonymousAccount account)
        {
            var claims = claimsBuilder.BuildForLocalUser(account);
            var token = securityTokenBuilder.Build(claims);
            return AuthenticationResult.Success(claims, token, account.AccountType);
        }
    }
}
