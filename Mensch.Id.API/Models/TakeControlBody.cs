using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models
{
    public class TakeControlBody
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string OwnershipSecret { get; set; }
    }
}
