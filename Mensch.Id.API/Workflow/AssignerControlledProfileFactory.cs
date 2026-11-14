using System;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow;

public static class AssignerControlledProfileFactory
{

    public static AssignerControlledProfile Create(
        MenschId menschId,
        string assignerAccountId)
    {
        var ownershipSecret = OwnershipSecret.Generate();
        return new AssignerControlledProfile(
            menschId,
            assignerAccountId,
            DateTime.UtcNow,
            ownershipSecret);
    }
}
