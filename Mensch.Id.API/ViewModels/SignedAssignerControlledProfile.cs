using System;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.ViewModels
{
    public class SignedAssignerControlledProfile : AssignerControlledProfile
    {
        public SignedAssignerControlledProfile(
            AssignerControlledProfile model,
            DateTime timestamp,
            string signature)
            : base(
                model.Id,
                model.AssignerAccountId, 
                model.CreationDate, 
                model.OwnershipSecret)
        {
            Timestamp = timestamp;
            Signature = signature;
        }

        public DateTime Timestamp { get; }
        public string Signature { get; }
    }
}
