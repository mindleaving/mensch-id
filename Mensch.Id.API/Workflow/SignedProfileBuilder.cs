using System;
using System.Collections.Generic;
using System.Security.Claims;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.ViewModels;

namespace Mensch.Id.API.Workflow
{
    public class SignedProfileBuilder
    {
        private readonly ClaimsSigner claimsSigner;

        public SignedProfileBuilder(
            ClaimsSigner claimsSigner)
        {
            this.claimsSigner = claimsSigner;
        }

        public SignedAssignerControlledProfile Build(
            AssignerControlledProfile profile)
        {
            var utcNow = DateTime.UtcNow;
            var claims = BuildClaims(profile, utcNow);
            var signature = claimsSigner.Sign(claims);
            return new SignedAssignerControlledProfile(profile, utcNow, signature);
        }

        public static List<Claim> BuildClaims(
            AssignerControlledProfile profile,
            DateTime timestamp)
        {
            return new List<Claim>
            {
                new(MenschIdClaimTypes.PersonIdClaimName, profile.Id),
                new(MenschIdClaimTypes.CertificateTimestamp, timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")),
                new(MenschIdClaimTypes.OwnershipSecret, profile.OwnershipSecret)
            };
        }
    }
}
