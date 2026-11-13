using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Models;
using Microsoft.AspNetCore.DataProtection;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Workflow;

public class AccountTokenServiceTest
{
    private const string AccountId = "8f6f1f7e-0b5c-4b8a-9d8f-3c1a2e4b5d6f";
    private AccountTokenService service;

    [SetUp]
    public void Setup()
    {
        service = new AccountTokenService(new EphemeralDataProtectionProvider());
    }

    [Test]
    [TestCase(AccountTokenPurpose.EmailVerification)]
    [TestCase(AccountTokenPurpose.PasswordReset)]
    public void GeneratedTokenIsValid(AccountTokenPurpose purpose)
    {
        var accountToken = service.Generate(purpose, AccountId);

        var isValid = service.Verify(
            purpose,
            accountToken.Token,
            AccountId,
            accountToken.Nonce);

        Assert.That(isValid, Is.True);
    }

    [Test]
    public void TokenDoesNotContainNonceOrAccountIdInPlainText()
    {
        var accountToken = service.Generate(AccountTokenPurpose.PasswordReset, AccountId);

        Assert.That(accountToken.Token, Does.Not.Contain(accountToken.Nonce));
        Assert.That(accountToken.Token, Does.Not.Contain(AccountId));
    }

    [Test]
    public void GeneratedNoncesAreUnique()
    {
        var first = service.Generate(AccountTokenPurpose.PasswordReset, AccountId);
        var second = service.Generate(AccountTokenPurpose.PasswordReset, AccountId);

        Assert.That(first.Nonce, Is.Not.EqualTo(second.Nonce));
        Assert.That(first.Token, Is.Not.EqualTo(second.Token));
    }

    [Test]
    public void TokenForOtherPurposeIsRejected()
    {
        var accountToken = service.Generate(AccountTokenPurpose.EmailVerification, AccountId);

        var isValid = service.Verify(
            AccountTokenPurpose.PasswordReset,
            accountToken.Token,
            AccountId,
            accountToken.Nonce);

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void TokenForOtherAccountIsRejected()
    {
        var accountToken = service.Generate(AccountTokenPurpose.PasswordReset, AccountId);

        var isValid = service.Verify(
            AccountTokenPurpose.PasswordReset,
            accountToken.Token,
            "other-account",
            accountToken.Nonce);

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void SupersededTokenIsRejected()
    {
        var oldToken = service.Generate(AccountTokenPurpose.PasswordReset, AccountId);
        var newToken = service.Generate(AccountTokenPurpose.PasswordReset, AccountId);

        var isValid = service.Verify(
            AccountTokenPurpose.PasswordReset,
            oldToken.Token,
            AccountId,
            newToken.Nonce);

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void UsedTokenIsRejectedAfterNonceWasCleared()
    {
        var accountToken = service.Generate(AccountTokenPurpose.PasswordReset, AccountId);

        var isValid = service.Verify(
            AccountTokenPurpose.PasswordReset,
            accountToken.Token,
            AccountId,
            null);

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void TamperedTokenIsRejected()
    {
        var accountToken = service.Generate(AccountTokenPurpose.PasswordReset, AccountId);
        var middleIndex = accountToken.Token.Length / 2;
        var replacementCharacter = accountToken.Token[middleIndex] == 'A' ? 'B' : 'A';
        var tamperedToken = accountToken.Token[..middleIndex] + replacementCharacter + accountToken.Token[(middleIndex + 1)..];

        var isValid = service.Verify(
            AccountTokenPurpose.PasswordReset,
            tamperedToken,
            AccountId,
            accountToken.Nonce);

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void TokenFromOtherKeyRingIsRejected()
    {
        var otherService = new AccountTokenService(new EphemeralDataProtectionProvider());
        var accountToken = otherService.Generate(AccountTokenPurpose.PasswordReset, AccountId);

        var isValid = service.Verify(
            AccountTokenPurpose.PasswordReset,
            accountToken.Token,
            AccountId,
            accountToken.Nonce);

        Assert.That(isValid, Is.False);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("garbage")]
    public void InvalidTokenIsRejected(string token)
    {
        var accountToken = service.Generate(AccountTokenPurpose.PasswordReset, AccountId);

        var isValid = service.Verify(
            AccountTokenPurpose.PasswordReset,
            token,
            AccountId,
            accountToken.Nonce);

        Assert.That(isValid, Is.False);
    }

    [Test]
    public void EmptyAccountIdCannotBeUsedForGeneration()
    {
        Assert.That(() => service.Generate(AccountTokenPurpose.PasswordReset, ""), Throws.ArgumentException);
    }
}
