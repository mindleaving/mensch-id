using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models
{
    public class LoginInformation
    {
        [JsonConstructor]
        public LoginInformation(string emailMenschIdOrUsername, string password)
        {
            EmailMenschIdOrUsername = emailMenschIdOrUsername;
            Password = password;
        }

        [Required]
        public string EmailMenschIdOrUsername { get; set; }

        [Required]
        public string Password { get; set; }
    }
}