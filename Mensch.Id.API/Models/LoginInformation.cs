using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models
{
    public class LoginInformation
    {
        [JsonConstructor]
        public LoginInformation(string username, string password)
        {
            Username = username;
            Password = password;
        }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}