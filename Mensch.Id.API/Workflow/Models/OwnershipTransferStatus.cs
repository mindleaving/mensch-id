namespace Mensch.Id.API.Workflow.Models;

public enum OwnershipTransferStatus
{
    Success,
    ProfileNotFound,
    InvalidOwnershipSecret,
    ControlledByOtherAccount,
    AccountNotEligible,
    AccountAlreadyHasProfile
}
