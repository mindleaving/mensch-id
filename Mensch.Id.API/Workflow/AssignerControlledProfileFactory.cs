using System;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow
{
    public static class AssignerControlledProfileFactory
    {
        public static AssignerControlledProfile Create(
            string menschId,
            string assignerAccountId)
        {
            var passwordGenerator = new TemporaryPasswordGenerator { AllowedCharacters = "ACDEFGHJKLMNPQRSTUVWXYZ2345679" };
            var ownershipSecret = passwordGenerator.Generate(length: 30);
            return new AssignerControlledProfile(
                menschId,
                assignerAccountId,
                DateTime.UtcNow,
                ownershipSecret);
        }
    }
}
