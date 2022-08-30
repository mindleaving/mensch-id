using System;

namespace Mensch.Id.API.Models
{
    public class AssignerControlledProfile : IId
    {
        public AssignerControlledProfile(
            string id,
            string assignerAccountId,
            DateTime creationDate,
            string ownershipSecret)
        {
            Id = id;
            AssignerAccountId = assignerAccountId;
            CreationDate = creationDate;
            OwnershipSecret = ownershipSecret;
        }

        public string Id { get; set; }
        public string AssignerAccountId { get; set; }
        public DateTime CreationDate { get; set; }
        public string OwnershipSecret { get; set; }
    }
}
