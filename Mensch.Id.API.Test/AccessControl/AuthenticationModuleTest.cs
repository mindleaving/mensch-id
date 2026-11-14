using System;
using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Mensch.Id.API.Test.AccessControl;

public class AuthenticationModuleTest
{
    private const string Password = "Password123!";
    private InMemoryAccountStore accountStore;
    private AccountCreator accountCreator;
    private AuthenticationModule authenticationModule;
    private PasswordHasher<IAccountWithPassword> passwordHasher;

    [SetUp]
    public void Setup()
    {
        var obscurer = TestKeys.CreateObscurer();
        accountStore = new InMemoryAccountStore(obscurer);
        passwordHasher = new PasswordHasher<IAccountWithPassword>();
        accountCreator = new AccountCreator(accountStore, obscurer, passwordHasher);
        authenticationModule = new AuthenticationModule(
            accountStore,
            passwordHasher,
            new ClaimBuilder());
    }

    private async Task<LocalAccount> CreateVerifiedLocalAccount(string personId = null)
    {
        var account = await accountCreator.CreateLocal("joe@example.org", Password, Language.en, personId);
        account.IsEmailVerified = true;
        await accountStore.StoreAsync(account);
        return account;
    }

    [Test]
    public async Task CorrectPasswordAuthenticatesAndBuildsSessionPrincipal()
    {
        var account = await CreateVerifiedLocalAccount("20000101-ACDEF");

        var result = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", Password));

        var claims = result.Principal.Claims.ToList();
        Assert.That(result.IsAuthenticated, Is.True);
        Assert.That(result.AccountType, Is.EqualTo(AccountType.Local));
        Assert.That(result.Principal.Identity!.AuthenticationType, Is.EqualTo(MenschIdAuthenticationSchemes.Session));
        Assert.That(ClaimsHelpers.GetAccountId(claims), Is.EqualTo(account.Id));
        Assert.That(ClaimsHelpers.GetPersonId(claims), Is.EqualTo("20000101-ACDEF"));
        Assert.That(claims.Any(x => x.Value == account.PasswordHash), Is.False);
    }

    [Test]
    public async Task WrongPasswordIsRejected()
    {
        await CreateVerifiedLocalAccount();

        var result = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", "wrong-password"));

        Assert.That(result.IsAuthenticated, Is.False);
        Assert.That(result.Principal, Is.Null);
        Assert.That(result.Error, Is.EqualTo(AuthenticationErrorType.InvalidUserOrPassword));
    }

    [Test]
    public async Task UnknownUserIsRejectedWithSameError()
    {
        await CreateVerifiedLocalAccount();

        var result = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("nobody@example.org", Password));

        Assert.That(result.IsAuthenticated, Is.False);
        Assert.That(result.Error, Is.EqualTo(AuthenticationErrorType.InvalidUserOrPassword));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public async Task EmptyPasswordIsRejected(string password)
    {
        await CreateVerifiedLocalAccount();

        var result = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", password));

        Assert.That(result.IsAuthenticated, Is.False);
    }

    [Test]
    public async Task UnverifiedEmailIsRejected()
    {
        await accountCreator.CreateLocal("joe@example.org", Password);

        var result = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", Password));

        Assert.That(result.IsAuthenticated, Is.False);
        Assert.That(result.Error, Is.EqualTo(AuthenticationErrorType.EmailNotVerified));
    }

    [Test]
    public async Task LoginByMenschIdWorksForAnonymousAccount()
    {
        await accountCreator.CreateLocalAnonymous(Password, Language.en, "20000101-ACDEF");

        var result = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("20000101-ACDEF", Password));

        Assert.That(result.IsAuthenticated, Is.True);
        Assert.That(result.AccountType, Is.EqualTo(AccountType.LocalAnonymous));
    }

    [Test]
    public async Task AssignerCanLoginByUsername()
    {
        await accountCreator.CreateAssigner("Clinic", "clinic", Password);

        var result = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("clinic", Password));

        Assert.That(result.IsAuthenticated, Is.True);
        Assert.That(result.AccountType, Is.EqualTo(AccountType.Assigner));
    }

    [Test]
    public async Task OutdatedPasswordHashIsUpgradedOnLogin()
    {
        var account = await CreateVerifiedLocalAccount();
        var oldHasherOptions = new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
            IterationCount = 1000
        };
        var oldHasher = new PasswordHasher<IAccountWithPassword>(new OptionsWrapper<PasswordHasherOptions>(oldHasherOptions));
        account.PasswordHash = oldHasher.HashPassword(account, Password);
        var oldPasswordHash = account.PasswordHash;
        await accountStore.StoreAsync(account);

        var result = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", Password));

        var storedAccount = (LocalAccount)await accountStore.GetByIdAsync(account.Id);
        Assert.That(result.IsAuthenticated, Is.True);
        Assert.That(storedAccount.PasswordHash, Is.Not.EqualTo(oldPasswordHash));
    }

    [Test]
    public async Task ChangedPasswordReplacesOldPassword()
    {
        var account = await CreateVerifiedLocalAccount();

        var isChanged = await authenticationModule.ChangePasswordAsync(account.Id, "NewPassword456!");
        var oldPasswordResult = await authenticationModule.AuthenticateLocalByAccountIdAsync(account.Id, Password);
        var newPasswordResult = await authenticationModule.AuthenticateLocalByAccountIdAsync(account.Id, "NewPassword456!");

        Assert.That(isChanged, Is.True);
        Assert.That(oldPasswordResult.IsAuthenticated, Is.False);
        Assert.That(newPasswordResult.IsAuthenticated, Is.True);
    }

    [Test]
    public async Task ExternalLoginAuthenticatesExistingExternalAccount()
    {
        var google = new LoginProvider("google");
        var account = await accountCreator.CreateExternal(google, "google-user-1");

        var result = await authenticationModule.AuthenticateExternalAsync(google, "google-user-1");

        var claims = result.Principal.Claims.ToList();
        Assert.That(result.IsAuthenticated, Is.True);
        Assert.That(result.AccountType, Is.EqualTo(AccountType.External));
        Assert.That(ClaimsHelpers.GetAccountId(claims), Is.EqualTo(account.Id));
        Assert.That(claims.Any(x => x.Value == "google-user-1"), Is.False);
    }

    [Test]
    public async Task ExternalLoginWithoutAccountFails()
    {
        var result = await authenticationModule.AuthenticateExternalAsync(new LoginProvider("google"), "unknown-user");

        Assert.That(result.IsAuthenticated, Is.False);
    }

    [Test]
    public async Task TooManyWrongPasswordsLockTheAccountUntilPasswordIsReset()
    {
        var account = await CreateVerifiedLocalAccount();
        for (var attempt = 0; attempt < AuthenticationModule.MaximumFailedLoginAttempts; attempt++)
        {
            await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", "wrong-password"));
        }

        var lockedResult = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", Password));
        await authenticationModule.ChangePasswordAsync(account.Id, "NewPassword456!");
        var resultAfterReset = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", "NewPassword456!"));

        Assert.That(lockedResult.Error, Is.EqualTo(AuthenticationErrorType.AccountLocked));
        Assert.That(resultAfterReset.IsAuthenticated, Is.True);
    }

    [Test]
    public async Task SuccessfulLoginResetsFailedAttempts()
    {
        var account = await CreateVerifiedLocalAccount();
        for (var attempt = 0; attempt < AuthenticationModule.MaximumFailedLoginAttempts - 1; attempt++)
        {
            await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", "wrong-password"));
        }

        await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", Password));
        await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", "wrong-password"));
        var result = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", Password));

        var storedAccount = (LocalAccount)await accountStore.GetByIdAsync(account.Id);
        Assert.That(result.IsAuthenticated, Is.True);
        Assert.That(storedAccount.LockoutEnd, Is.Null);
        Assert.That(storedAccount.FailedLoginAttempts, Is.EqualTo(0));
    }
}
