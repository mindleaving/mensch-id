using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models
{
    public class TakeControlBody
    {
        [Required]
        public MenschId? Id { get; set; }

        [Required]
        public OwnershipSecret? OwnershipSecret { get; set; }
    }
}
