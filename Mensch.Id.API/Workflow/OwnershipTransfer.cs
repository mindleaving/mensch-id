using System;
using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow.Models;

namespace Mensch.Id.API.Workflow;

public class OwnershipTransfer
{
    private readonly IAccountStore accountStore;
    private readonly IStore<Person> personStore;
    private readonly IStore<AssignerControlledProfile> assignerControlledProfileStore;

    public OwnershipTransfer(
        IAccountStore accountStore,
        IStore<Person> personStore,
        IStore<AssignerControlledProfile> assignerControlledProfileStore)
    {
        this.accountStore = accountStore;
        this.personStore = personStore;
        this.assignerControlledProfileStore = assignerControlledProfileStore;
    }

    public async Task<OwnershipTransferResult> TakeControl(
        Account account,
        MenschId menschIdValue,
        OwnershipSecret ownershipSecret)
    {
        var menschId = menschIdValue.Value;
        if (account is not PersonAccount personAccount)
            return OwnershipTransferResult.Failed(OwnershipTransferStatus.AccountNotEligible);
        if (personAccount.PersonId == menschId)
        {
            var existingProfile = await personStore.GetByIdAsync(menschId);
            return OwnershipTransferResult.Success(existingProfile);
        }
        if (personAccount.PersonId != null)
            return OwnershipTransferResult.Failed(OwnershipTransferStatus.AccountAlreadyHasProfile);

        var assignerControlledProfile = await assignerControlledProfileStore.GetByIdAsync(menschId);
        if (assignerControlledProfile == null)
            return OwnershipTransferResult.Failed(OwnershipTransferStatus.ProfileNotFound);
        if (!IsOwnershipSecretValid(ownershipSecret, assignerControlledProfile.OwnershipSecret))
            return OwnershipTransferResult.Failed(OwnershipTransferStatus.InvalidOwnershipSecret);
        var owningAccounts = await accountStore.GetAllForMenschIdAsync(menschId);
        if (owningAccounts.Any())
            return OwnershipTransferResult.Failed(OwnershipTransferStatus.ControlledByOtherAccount);

        var profile = await personStore.GetByIdAsync(menschId);
        if (profile == null)
        {
            profile = new Person(menschId, DateTime.UtcNow, personAccount.Id);
            await personStore.StoreAsync(profile);
        }
        personAccount.PersonId = menschId;
        await accountStore.StoreAsync(personAccount);
        await assignerControlledProfileStore.DeleteAsync(menschId);
        return OwnershipTransferResult.Success(profile);
    }

    private static bool IsOwnershipSecretValid(
        OwnershipSecret providedSecret,
        string storedSecret)
    {
        if (!OwnershipSecret.IsValid(storedSecret))
            return false;
        var expectedSecret = new OwnershipSecret(storedSecret);
        return providedSecret == expectedSecret;
    }
}
