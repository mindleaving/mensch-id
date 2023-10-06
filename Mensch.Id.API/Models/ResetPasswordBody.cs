using System.ComponentModel.DataAnnotations;

namespace Mensch.Id.API.Models
{
    public class ResetPasswordBody
    {
        [Required]
        public string AccountId { get; set; }

        [Required]
        public string ResetToken { get; set; }

        [Required]
        public string Password { get; set; }
    }
}