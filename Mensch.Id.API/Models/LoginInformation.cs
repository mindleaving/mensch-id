using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models
{
    public class LoginInformation
    {
        [JsonConstructor]
        public LoginInformation(string email, string password)
        {
            Email = email;
            Password = password;
        }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [TypescriptIsOptional]
        public bool RegisterIfNotExists { get; set; }
    }
}