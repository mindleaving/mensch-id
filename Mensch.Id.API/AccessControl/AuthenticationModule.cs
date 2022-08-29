using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;

namespace Mensch.Id.API.AccessControl
{
    public class AuthenticationModule : IAuthenticationModule
    {
        private readonly IAccountStore accountStore;
        private readonly ISecurityTokenBuilder securityTokenBuilder;

        public AuthenticationModule(
            IAccountStore accountStore,
            ISecurityTokenBuilder securityTokenBuilder)
        {
            this.accountStore = accountStore;
            this.securityTokenBuilder = securityTokenBuilder;
        }

        public async Task<bool> ChangePasswordAsync(
            string email,
            string password,
            bool changePasswordOnNextLogin = false)
        {
            var matchingAccount = await accountStore.GetLocalByEmailAsync(email);
            if (matchingAccount == null)
                return false;
            var passwordHash = PasswordHasher.Hash(password, matchingAccount.Salt);
            var passwordBase64 = Convert.ToBase64String(passwordHash);

            var result = await accountStore.ChangePasswordAsync(email, passwordBase64);
            return result.IsSuccess;
        }

        public async Task<AuthenticationResult> AuthenticateLocalAsync(LoginInformation loginInformation)
        {
            if(string.IsNullOrEmpty(loginInformation.Password))
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidPassword);
            var account = await accountStore.GetLocalByEmailOrMenschIdAsync(loginInformation.EmailOrMenschId);
            if(account == null)
                return AuthenticationResult.Failed(AuthenticationErrorType.UserNotFound);
            var isMatch = HashComparer.Compare(loginInformation.Password, account.PasswordHash, account.Salt);
            if (!isMatch)
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidPassword);
            if (account is LocalAccount localAccount)
            {
                if(!localAccount.IsEmailVerified)
                    return AuthenticationResult.Failed(AuthenticationErrorType.EmailNotVerified);
            }
            return BuildSecurityTokenForUser(account);
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
            return AuthenticationResult.Success(token);
        }

        public AuthenticationResult BuildSecurityTokenForUser(LocalAnonymousAccount account)
        {
            var token = securityTokenBuilder.BuildForLocalUser(account);
            return AuthenticationResult.Success(token);
        }
    }
}
