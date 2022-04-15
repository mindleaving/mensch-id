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
            string username,
            string password,
            bool changePasswordOnNextLogin = false)
        {
            var matchingAccount = await accountStore.GetLocalByIdAsync(username);
            if (matchingAccount == null)
                return false;
            var saltBytes = Convert.FromBase64String(matchingAccount.Salt);
            var passwordHash = PasswordHasher.Hash(password, saltBytes, PasswordHasher.RecommendedHashLength);
            var passwordBase64 = Convert.ToBase64String(passwordHash);

            var result = await accountStore.ChangePasswordAsync(username, passwordBase64);
            return result.IsSuccess;
        }

        public async Task<AuthenticationResult> AuthenticateLocalAsync(LoginInformation loginInformation)
        {
            if(string.IsNullOrEmpty(loginInformation.Password))
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidPassword);
            var account = await accountStore.GetLocalByIdAsync(loginInformation.Username);
            if(account == null)
                return AuthenticationResult.Failed(AuthenticationErrorType.UserNotFound);
            var salt = Convert.FromBase64String(account.Salt);
            var storedPasswordHash = Convert.FromBase64String(account.PasswordHash);
            var providedPasswordHash = PasswordHasher.Hash(loginInformation.Password, salt, 8 * storedPasswordHash.Length);
            var isMatch = HashComparer.Compare(providedPasswordHash, storedPasswordHash);
            if (!isMatch)
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidPassword);

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

        private AuthenticationResult BuildSecurityTokenForUser(LocalAccount account)
        {
            var token = securityTokenBuilder.BuildForLocalUser(account);
            return AuthenticationResult.Success(token);
        }
    }
}
