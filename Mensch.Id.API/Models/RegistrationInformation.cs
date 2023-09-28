using System.ComponentModel.DataAnnotations;
using Mensch.Id.API.Models.AccessControl;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models
{
    public class RegistrationInformation
    {
        [Required]
        public AccountType AccountType { get; set; }
        [TypescriptIsOptional]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public Language? PreferedLanguage { get; set; }
    }
}