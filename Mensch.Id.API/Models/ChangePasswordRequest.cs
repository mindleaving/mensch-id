using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models
{
    public class ChangePasswordRequest
    {
        [Required]
        public string AccountId { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
