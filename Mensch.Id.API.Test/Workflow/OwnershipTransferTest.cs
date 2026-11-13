using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Models;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Workflow;

public class OwnershipTransferTest
{
    private const string MenschId = "20000101-ACDEF";
    private const string AssignerAccountId = "assigner-1";
    private InMemoryAccountStore accountStore;
    private InMemoryStore<Person> personStore;
    private InMemoryStore<AssignerControlledProfile> assignerControlledProfileStore;
    private OwnershipTransfer ownershipTransfer;
    private AssignerControlledProfile assignerControlledProfile;

    [SetUp]
    public async Task Setup()
    {
        accountStore = new InMemoryAccountStore(TestKeys.CreateObscurer());
        personStore = new InMemoryStore<Person>();
        assignerControlledProfileStore = new InMemoryStore<AssignerControlledProfile>();
        ownershipTransfer = new OwnershipTransfer(accountStore, personStore, assignerControlledProfileStore);

        assignerControlledProfile = AssignerControlledProfileFactory.Create(MenschId, AssignerAccountId);
        await assignerControlledProfileStore.StoreAsync(assignerControlledProfile);
        await personStore.StoreAsync(new Person(MenschId, DateTime.UtcNow, AssignerAccountId));
    }

    private async Task<LocalAccount> CreateUserAccount(string personId = null)
    {
        var account = new LocalAccount
        {
            Id = Guid.NewGuid().ToString(),
            Email = $"{Guid.NewGuid()}@example.org",
            IsEmailVerified = true,
            PersonId = personId
        };
        await accountStore.StoreAsync(account);
        return account;
    }

    [Test]
    public async Task CorrectSecretTransfersControl()
    {
        var account = await CreateUserAccount();

        var result = await ownershipTransfer.TakeControl(account, MenschId, assignerControlledProfile.OwnershipSecret);

        var storedAccount = (LocalAccount)await accountStore.GetByIdAsync(account.Id);
        var remainingAssignerProfile = await assignerControlledProfileStore.GetByIdAsync(MenschId);
        Assert.That(result.Status, Is.EqualTo(OwnershipTransferStatus.Success));
        Assert.That(result.Profile.Id, Is.EqualTo(MenschId));
        Assert.That(storedAccount.PersonId, Is.EqualTo(MenschId));
        Assert.That(remainingAssignerProfile, Is.Null);
    }

    [Test]
    public async Task MissingPersonIsCreatedDuringTransfer()
    {
        await personStore.DeleteAsync(MenschId);
        var account = await CreateUserAccount();

        var result = await ownershipTransfer.TakeControl(account, MenschId, assignerControlledProfile.OwnershipSecret);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await personStore.ExistsAsync(MenschId), Is.True);
    }

    [Test]
    public async Task WrongSecretIsRejectedAndNothingChanges()
    {
        var account = await CreateUserAccount();
        var wrongSecret = new string('A', AssignerControlledProfileFactory.OwnershipSecretLength);

        var result = await ownershipTransfer.TakeControl(account, MenschId, wrongSecret);

        var storedAccount = (LocalAccount)await accountStore.GetByIdAsync(account.Id);
        Assert.That(result.Status, Is.EqualTo(OwnershipTransferStatus.InvalidOwnershipSecret));
        Assert.That(storedAccount.PersonId, Is.Null);
        Assert.That(await assignerControlledProfileStore.ExistsAsync(MenschId), Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public async Task EmptySecretIsRejected(string secret)
    {
        var account = await CreateUserAccount();

        var result = await ownershipTransfer.TakeControl(account, MenschId, secret);

        Assert.That(result.Status, Is.EqualTo(OwnershipTransferStatus.InvalidOwnershipSecret));
    }

    [Test]
    public async Task SecretIsCaseSensitive()
    {
        var account = await CreateUserAccount();
        var lowerCaseSecret = assignerControlledProfile.OwnershipSecret.ToLowerInvariant();

        var result = await ownershipTransfer.TakeControl(account, MenschId, lowerCaseSecret);

        Assert.That(result.Status, Is.EqualTo(OwnershipTransferStatus.InvalidOwnershipSecret));
    }

    [Test]
    public async Task UnknownIdIsNotFound()
    {
        var account = await CreateUserAccount();

        var result = await ownershipTransfer.TakeControl(account, "20000101-ZZZZZ", assignerControlledProfile.OwnershipSecret);

        Assert.That(result.Status, Is.EqualTo(OwnershipTransferStatus.ProfileNotFound));
    }

    [Test]
    public async Task SecretCannotBeReusedAfterTransfer()
    {
        var firstAccount = await CreateUserAccount();
        var secondAccount = await CreateUserAccount();
        await ownershipTransfer.TakeControl(firstAccount, MenschId, assignerControlledProfile.OwnershipSecret);

        var result = await ownershipTransfer.TakeControl(secondAccount, MenschId, assignerControlledProfile.OwnershipSecret);

        var storedSecondAccount = (LocalAccount)await accountStore.GetByIdAsync(secondAccount.Id);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(storedSecondAccount.PersonId, Is.Null);
    }

    [Test]
    public async Task IdAlreadyControlledByOtherAccountIsRejected()
    {
        await CreateUserAccount(MenschId);
        var account = await CreateUserAccount();

        var result = await ownershipTransfer.TakeControl(account, MenschId, assignerControlledProfile.OwnershipSecret);

        Assert.That(result.Status, Is.EqualTo(OwnershipTransferStatus.ControlledByOtherAccount));
    }

    [Test]
    public async Task AccountWithOtherProfileIsRejected()
    {
        var account = await CreateUserAccount("19990101-ACDEF");

        var result = await ownershipTransfer.TakeControl(account, MenschId, assignerControlledProfile.OwnershipSecret);

        Assert.That(result.Status, Is.EqualTo(OwnershipTransferStatus.AccountAlreadyHasProfile));
        Assert.That(await assignerControlledProfileStore.ExistsAsync(MenschId), Is.True);
    }

    [Test]
    public async Task AccountAlreadyControllingIdSucceedsWithoutChanges()
    {
        var account = await CreateUserAccount(MenschId);

        var result = await ownershipTransfer.TakeControl(account, MenschId, "irrelevant");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Profile.Id, Is.EqualTo(MenschId));
    }

    [Test]
    public async Task AssignerAccountIsNotEligible()
    {
        var assignerAccount = new AssignerAccount { Id = AssignerAccountId, Username = "assigner" };

        var result = await ownershipTransfer.TakeControl(assignerAccount, MenschId, assignerControlledProfile.OwnershipSecret);

        Assert.That(result.Status, Is.EqualTo(OwnershipTransferStatus.AccountNotEligible));
    }

    [Test]
    public async Task MissingAccountIsNotEligible()
    {
        var result = await ownershipTransfer.TakeControl(null, MenschId, assignerControlledProfile.OwnershipSecret);

        Assert.That(result.Status, Is.EqualTo(OwnershipTransferStatus.AccountNotEligible));
    }

    [Test]
    public async Task ExternalAccountCanTakeControl()
    {
        var externalAccount = new ExternalAccount
        {
            Id = Guid.NewGuid().ToString(),
            LoginProvider = new LoginProvider("google"),
            ExternalId = "obscured"
        };
        await accountStore.StoreAsync(externalAccount);

        var result = await ownershipTransfer.TakeControl(externalAccount, MenschId, assignerControlledProfile.OwnershipSecret);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(externalAccount.PersonId, Is.EqualTo(MenschId));
    }

    [Test]
    public void GeneratedOwnershipSecretHasExpectedFormat()
    {
        var profile = AssignerControlledProfileFactory.Create(MenschId, AssignerAccountId);
        var otherProfile = AssignerControlledProfileFactory.Create(MenschId, AssignerAccountId);

        Assert.That(profile.OwnershipSecret, Has.Length.EqualTo(AssignerControlledProfileFactory.OwnershipSecretLength));
        Assert.That(profile.OwnershipSecret, Does.Match("^[ACDEFGHJKLMNPQRSTUVWXYZ2345679]+$"));
        Assert.That(profile.OwnershipSecret, Is.Not.EqualTo(otherProfile.OwnershipSecret));
        Assert.That(profile.AssignerAccountId, Is.EqualTo(AssignerAccountId));
    }
}
