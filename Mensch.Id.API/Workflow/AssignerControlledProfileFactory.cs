using System;
using System.Security.Cryptography;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow;

public static class AssignerControlledProfileFactory
{
    public const int OwnershipSecretLength = 30;
    private const string OwnershipSecretCharacters = "ACDEFGHJKLMNPQRSTUVWXYZ2345679";

    public static AssignerControlledProfile Create(
        string menschId,
        string assignerAccountId)
    {
        var ownershipSecret = RandomNumberGenerator.GetString(OwnershipSecretCharacters, OwnershipSecretLength);
        return new AssignerControlledProfile(
            menschId,
            assignerAccountId,
            DateTime.UtcNow,
            ownershipSecret);
    }
}
