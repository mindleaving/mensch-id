using System;

namespace Mensch.Id.API.Models;

public class AssignerControlledProfile : IId<MenschId>
{
    public AssignerControlledProfile(
        MenschId id,
        string assignerAccountId,
        DateTime creationDate,
        OwnershipSecret ownershipSecret)
    {
        Id = id;
        AssignerAccountId = assignerAccountId;
        CreationDate = creationDate;
        OwnershipSecret = ownershipSecret;
    }

    public MenschId Id { get; set; }
    public string AssignerAccountId { get; set; }
    public DateTime CreationDate { get; set; }
    public OwnershipSecret OwnershipSecret { get; set; }
}
