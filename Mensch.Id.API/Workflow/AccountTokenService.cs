using System;
using System.Security.Cryptography;
using System.Text;
using Mensch.Id.API.Workflow.Models;
using Microsoft.AspNetCore.DataProtection;

namespace Mensch.Id.API.Workflow;

public interface IAccountTokenService
{
    AccountToken Generate(
        AccountTokenPurpose purpose,
        string accountId);

    bool Verify(
        AccountTokenPurpose purpose,
        string token,
        string accountId,
        string storedNonce);
}

public class AccountTokenService : IAccountTokenService
{
    public static readonly TimeSpan EmailVerificationLifetime = TimeSpan.FromDays(7);
    public static readonly TimeSpan PasswordResetLifetime = TimeSpan.FromHours(2);
    private const int NonceLength = 32;
    private const char PayloadSeparator = ':';
    private readonly IDataProtectionProvider dataProtectionProvider;

    public AccountTokenService(IDataProtectionProvider dataProtectionProvider)
    {
        this.dataProtectionProvider = dataProtectionProvider;
    }

    public AccountToken Generate(
        AccountTokenPurpose purpose,
        string accountId)
    {
        if (string.IsNullOrEmpty(accountId))
            throw new ArgumentException("Account ID must not be empty", nameof(accountId));
        var nonce = RandomNumberGenerator.GetHexString(NonceLength);
        var protector = CreateProtector(purpose);
        var lifetime = GetLifetime(purpose);
        var token = protector.Protect(accountId + PayloadSeparator + nonce, lifetime);
        return new AccountToken(token, nonce);
    }

    public bool Verify(
        AccountTokenPurpose purpose,
        string token,
        string accountId,
        string storedNonce)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(storedNonce))
            return false;
        var protector = CreateProtector(purpose);
        string payload;
        try
        {
            payload = protector.Unprotect(token);
        }
        catch (CryptographicException)
        {
            return false;
        }
        var separatorIndex = payload.LastIndexOf(PayloadSeparator);
        if (separatorIndex < 0)
            return false;
        var tokenAccountId = payload[..separatorIndex];
        var tokenNonce = payload[(separatorIndex + 1)..];
        if (!string.Equals(tokenAccountId, accountId, StringComparison.Ordinal))
            return false;
        var tokenNonceBytes = Encoding.UTF8.GetBytes(tokenNonce);
        var storedNonceBytes = Encoding.UTF8.GetBytes(storedNonce);
        return CryptographicOperations.FixedTimeEquals(tokenNonceBytes, storedNonceBytes);
    }

    private ITimeLimitedDataProtector CreateProtector(AccountTokenPurpose purpose)
    {
        return dataProtectionProvider
            .CreateProtector($"Mensch.Id.API.AccountToken.{purpose}")
            .ToTimeLimitedDataProtector();
    }

    private static TimeSpan GetLifetime(AccountTokenPurpose purpose)
    {
        return purpose switch
        {
            AccountTokenPurpose.EmailVerification => EmailVerificationLifetime,
            AccountTokenPurpose.PasswordReset => PasswordResetLifetime,
            _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, null)
        };
    }
}
