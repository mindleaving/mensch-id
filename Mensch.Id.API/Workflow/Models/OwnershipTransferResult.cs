using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow.Models;

public class OwnershipTransferResult
{
    private OwnershipTransferResult(
        OwnershipTransferStatus status,
        Person profile)
    {
        Status = status;
        Profile = profile;
    }

    public static OwnershipTransferResult Success(Person profile) => new(OwnershipTransferStatus.Success, profile);
    public static OwnershipTransferResult Failed(OwnershipTransferStatus status) => new(status, null);

    public OwnershipTransferStatus Status { get; }
    public Person Profile { get; }
    public bool IsSuccess => Status == OwnershipTransferStatus.Success;
}
