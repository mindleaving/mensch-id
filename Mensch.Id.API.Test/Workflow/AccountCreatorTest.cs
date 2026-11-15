using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Workflow;

public class AccountCreatorTest
{
    private static readonly LoginProvider Google = new("google");
    private InMemoryAccountStore accountStore;
    private PasswordHasher<IAccountWithPassword> passwordHasher;
    private AccountCreator accountCreator;

    [SetUp]
    public void Setup()
    {
        var obscurer = TestKeys.CreateObscurer();
        accountStore = new InMemoryAccountStore(obscurer);
        passwordHasher = new PasswordHasher<IAccountWithPassword>();
        accountCreator = new AccountCreator(accountStore, obscurer, passwordHasher);
    }

    [Test]
    public async Task LocalAccountIsCreatedWithHashedPasswordAndUnverifiedEmail()
    {
        var account = await accountCreator.CreateLocal("joe@example.org", "Password123!");

        var storedAccount = (LocalAccount)await accountStore.GetByIdAsync(account.Id);
        var verificationResult = passwordHasher.VerifyHashedPassword(storedAccount, storedAccount.PasswordHash, "Password123!");
        Assert.That(storedAccount.Email, Is.EqualTo("joe@example.org"));
        Assert.That(storedAccount.IsEmailVerified, Is.False);
        Assert.That(storedAccount.PasswordHash, Does.Not.Contain("Password123!"));
        Assert.That(verificationResult, Is.EqualTo(PasswordVerificationResult.Success));
        Assert.That(storedAccount.PersonId, Is.Null);
    }

    [Test]
    public async Task DuplicateEmailIsRejected()
    {
        await accountCreator.CreateLocal("joe@example.org", "Password123!");

        Assert.That(
            async () => await accountCreator.CreateLocal("joe@example.org", "OtherPassword1"),
            Throws.TypeOf<DuplicateNameException>());
    }

    [Test]
    public async Task LocalAnonymousAccountCanBeLinkedToMenschId()
    {
        var account = await accountCreator.CreateLocalAnonymous("Password123!", Language.de, "20000101-ACDEF");

        Assert.That(account.PersonId, Is.EqualTo("20000101-ACDEF"));
        Assert.That(account.PreferedLanguage, Is.EqualTo(Language.de));
        Assert.That(accountStore.Items, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task SecondLocalAnonymousAccountForSameMenschIdIsRejected()
    {
        await accountCreator.CreateLocalAnonymous("Password123!", Language.en, "20000101-ACDEF");

        Assert.That(
            async () => await accountCreator.CreateLocalAnonymous("Password456!", Language.en, "20000101-ACDEF"),
            Throws.TypeOf<DuplicateNameException>());
    }

    [Test]
    public async Task ExternalAccountStoresOnlyObscuredExternalId()
    {
        var account = await accountCreator.CreateExternal(Google, "google-user-1");

        var storedAccount = (ExternalAccount)accountStore.Items.Single();
        Assert.That(storedAccount.Id, Is.EqualTo(account.Id));
        Assert.That(storedAccount.LoginProvider, Is.EqualTo(Google));
        Assert.That(storedAccount.ExternalId, Is.Not.EqualTo("google-user-1"));
        Assert.That(storedAccount.ExternalId, Does.Not.Contain("google-user-1"));
        Assert.That(storedAccount.PersonId, Is.Null);
    }

    [Test]
    public async Task ExternalAccountCanBeFoundByExternalId()
    {
        var account = await accountCreator.CreateExternal(Google, "google-user-1");

        var foundAccount = await accountStore.GetExternalByIdAsync(Google, "google-user-1");
        var accountOfOtherProvider = await accountStore.GetExternalByIdAsync(new LoginProvider("microsoft"), "google-user-1");

        Assert.That(foundAccount.Id, Is.EqualTo(account.Id));
        Assert.That(accountOfOtherProvider, Is.Null);
    }

    [Test]
    public async Task DuplicateExternalAccountIsRejected()
    {
        await accountCreator.CreateExternal(Google, "google-user-1");

        Assert.That(
            async () => await accountCreator.CreateExternal(Google, "google-user-1"),
            Throws.TypeOf<DuplicateNameException>());
    }

    [Test]
    public async Task AssignerAccountIsCreated()
    {
        var account = await accountCreator.CreateAssigner("Clinic", "clinic@example.org", "Password123!");

        Assert.That(account.Username, Is.EqualTo("clinic@example.org"));
        Assert.That(account.AccountType, Is.EqualTo(AccountType.Assigner));
        Assert.That(
            async () => await accountCreator.CreateAssigner("Clinic 2", "clinic@example.org", "Password456!"),
            Throws.TypeOf<DuplicateNameException>());
    }
}
