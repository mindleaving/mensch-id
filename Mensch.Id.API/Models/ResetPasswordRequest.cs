using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
