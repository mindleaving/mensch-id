﻿using System;
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
        private readonly IPasswordHasher<IAccountWithPassword> passwordHasher;
        private readonly IClaimBuilder claimsBuilder;

        public AuthenticationModule(
            IAccountStore accountStore,
            ISecurityTokenBuilder securityTokenBuilder,
            IPasswordHasher<IAccountWithPassword> passwordHasher,
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
            var matchingAccount = await accountStore.GetByIdAsync(accountId);
            if (matchingAccount is not IAccountWithPassword accountWithPassword) 
                return false;
            var passwordHash = passwordHasher.HashPassword(accountWithPassword, password);
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
                if(account is not IAccountWithPassword accountWithPassword)
                    continue;

                var verificationResult = passwordHasher.VerifyHashedPassword(accountWithPassword, accountWithPassword.PasswordHash, loginInformation.Password);
                if (verificationResult == PasswordVerificationResult.Failed)
                    continue;
                if (verificationResult != PasswordVerificationResult.Success && verificationResult != PasswordVerificationResult.SuccessRehashNeeded)
                    throw new ArgumentOutOfRangeException(nameof(verificationResult), $"Invalid verification result '{verificationResult}'");
                if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    accountWithPassword.PasswordHash = passwordHasher.HashPassword(accountWithPassword, loginInformation.Password);
                    await accountStore.StoreAsync((Account)accountWithPassword);
                }
                switch (account)
                {
                    case LocalAnonymousAccount localAnonymousAccount:
                    {
                        if (localAnonymousAccount is LocalAccount localAccount)
                        {
                            if(!localAccount.IsEmailVerified)
                                return AuthenticationResult.Failed(AuthenticationErrorType.EmailNotVerified);
                        }
                        return BuildSecurityTokenForUser(localAnonymousAccount);
                    }
                    case ProfessionalAccount professionalAccount:
                        return BuildSecurityTokenForUser(professionalAccount);
                    default:
                        throw new NotSupportedException($"Cannot build security token for {account.GetType().Name}");
                }
            }
            return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
        }

        public async Task<AuthenticationResult> AuthenticateLocalByAccountIdAsync(
            string accountId,
            string password)
        {
            if(string.IsNullOrEmpty(password))
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
            var account = await accountStore.GetByIdAsync(accountId);
            if (account is not IAccountWithPassword accountWithPassword)
                return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);

            var verificationResult = passwordHasher.VerifyHashedPassword(accountWithPassword, accountWithPassword.PasswordHash, password);
            switch (verificationResult)
            {
                case PasswordVerificationResult.Failed:
                    return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
                case PasswordVerificationResult.SuccessRehashNeeded:
                    accountWithPassword.PasswordHash = passwordHasher.HashPassword(accountWithPassword, password);
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
            return account is LocalAnonymousAccount localAnonymousAccount ? BuildSecurityTokenForUser(localAnonymousAccount)
                    : account is ProfessionalAccount professionalAccount ? BuildSecurityTokenForUser(professionalAccount)
                    : throw new NotSupportedException($"Cannot build security token for {account.GetType().Name}");
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

        public AuthenticationResult BuildSecurityTokenForUser(ProfessionalAccount account)
        {
            var claims = claimsBuilder.BuildForProfessionalUser(account);
            var token = securityTokenBuilder.Build(claims);
            return AuthenticationResult.Success(claims, token, account.AccountType);
        }
    }
}
