using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Identity;

namespace Mensch.Id.API.AccessControl;

public class AuthenticationModule : IAuthenticationModule
{
    public const int MaximumFailedLoginAttempts = 10;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IAccountStore accountStore;
    private readonly IPasswordHasher<IAccountWithPassword> passwordHasher;
    private readonly IClaimBuilder claimBuilder;

    public AuthenticationModule(
        IAccountStore accountStore,
        IPasswordHasher<IAccountWithPassword> passwordHasher,
        IClaimBuilder claimBuilder)
    {
        this.accountStore = accountStore;
        this.passwordHasher = passwordHasher;
        this.claimBuilder = claimBuilder;
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
        if (!result.IsSuccess)
            return false;
        var updatedAccount = await accountStore.GetByIdAsync(accountId);
        if (updatedAccount is IAccountWithPassword updatedAccountWithPassword)
            await ResetFailedLogins(updatedAccount, updatedAccountWithPassword);
        return true;
    }

    public async Task<AuthenticationResult> AuthenticateLocalByEmailMenschIdOrUsernameAsync(LoginInformation loginInformation)
    {
        if (string.IsNullOrEmpty(loginInformation.Password))
            return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
        var accounts = await accountStore.GetLocalsByEmailMenschIdOrUsernameAsync(loginInformation.EmailMenschIdOrUsername);
        var isAnyAccountLocked = false;
        foreach (var account in accounts)
        {
            if (account is not IAccountWithPassword accountWithPassword)
                continue;
            if (IsLockedOut(accountWithPassword))
            {
                isAnyAccountLocked = true;
                continue;
            }
            var isPasswordValid = await VerifyPassword(account, accountWithPassword, loginInformation.Password);
            if (!isPasswordValid)
                continue;
            if (account is LocalAccount { IsEmailVerified: false })
                return AuthenticationResult.Failed(AuthenticationErrorType.EmailNotVerified);
            return BuildAuthenticationResult(account);
        }
        return AuthenticationResult.Failed(isAnyAccountLocked ? AuthenticationErrorType.AccountLocked : AuthenticationErrorType.InvalidUserOrPassword);
    }

    public async Task<AuthenticationResult> AuthenticateLocalByAccountIdAsync(
        string accountId,
        string password)
    {
        if (string.IsNullOrEmpty(password))
            return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
        var account = await accountStore.GetByIdAsync(accountId);
        if (account is not IAccountWithPassword accountWithPassword)
            return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
        if (IsLockedOut(accountWithPassword))
            return AuthenticationResult.Failed(AuthenticationErrorType.AccountLocked);
        var isPasswordValid = await VerifyPassword(account, accountWithPassword, password);
        if (!isPasswordValid)
            return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
        if (account is LocalAccount { IsEmailVerified: false })
            return AuthenticationResult.Failed(AuthenticationErrorType.EmailNotVerified);
        return BuildAuthenticationResult(account);
    }

    public async Task<AuthenticationResult> AuthenticateExternalAsync(
        LoginProvider loginProvider,
        string externalId)
    {
        if (loginProvider == LoginProvider.Local)
            throw new ArgumentException("This method is intended for external login providers only", nameof(loginProvider));
        var account = await accountStore.GetExternalByIdAsync(loginProvider, externalId);
        if (account == null)
            return AuthenticationResult.Failed(AuthenticationErrorType.InvalidUserOrPassword);
        return BuildAuthenticationResult(account);
    }

    public AuthenticationResult BuildAuthenticationResult(Account account)
    {
        var principal = claimBuilder.BuildPrincipal(account, MenschIdAuthenticationSchemes.Session);
        return AuthenticationResult.Success(account, principal);
    }

    private async Task<bool> VerifyPassword(
        Account account,
        IAccountWithPassword accountWithPassword,
        string password)
    {
        if (string.IsNullOrEmpty(accountWithPassword.PasswordHash))
            return false;
        var verificationResult = passwordHasher.VerifyHashedPassword(accountWithPassword, accountWithPassword.PasswordHash, password);
        switch (verificationResult)
        {
            case PasswordVerificationResult.Failed:
                await RegisterFailedLogin(account, accountWithPassword);
                return false;
            case PasswordVerificationResult.Success:
                await ResetFailedLogins(account, accountWithPassword);
                return true;
            case PasswordVerificationResult.SuccessRehashNeeded:
                accountWithPassword.PasswordHash = passwordHasher.HashPassword(accountWithPassword, password);
                accountWithPassword.FailedLoginAttempts = 0;
                accountWithPassword.LockoutEnd = null;
                await accountStore.StoreAsync(account);
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(verificationResult), $"Invalid verification result '{verificationResult}'");
        }
    }

    private static bool IsLockedOut(IAccountWithPassword accountWithPassword)
    {
        return accountWithPassword.LockoutEnd.HasValue && accountWithPassword.LockoutEnd.Value > DateTime.UtcNow;
    }

    private async Task RegisterFailedLogin(
        Account account,
        IAccountWithPassword accountWithPassword)
    {
        var failedLoginAttempts = accountWithPassword.FailedLoginAttempts + 1;
        if (failedLoginAttempts >= MaximumFailedLoginAttempts)
        {
            accountWithPassword.FailedLoginAttempts = 0;
            accountWithPassword.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
        }
        else
        {
            accountWithPassword.FailedLoginAttempts = failedLoginAttempts;
        }
        await accountStore.StoreAsync(account);
    }

    private async Task ResetFailedLogins(
        Account account,
        IAccountWithPassword accountWithPassword)
    {
        if (accountWithPassword.FailedLoginAttempts == 0 && accountWithPassword.LockoutEnd == null)
            return;
        accountWithPassword.FailedLoginAttempts = 0;
        accountWithPassword.LockoutEnd = null;
        await accountStore.StoreAsync(account);
    }
}
