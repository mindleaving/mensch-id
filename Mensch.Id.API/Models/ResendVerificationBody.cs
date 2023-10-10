using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models
{
    public class ResendVerificationBody
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
