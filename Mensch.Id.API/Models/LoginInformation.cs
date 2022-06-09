using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models
{
    public class LoginInformation
    {
        [JsonConstructor]
        public LoginInformation(string emailOrMenschId, string password)
        {
            EmailOrMenschId = emailOrMenschId;
            Password = password;
        }

        [Required]
        public string EmailOrMenschId { get; set; }

        [Required]
        public string Password { get; set; }
    }
}