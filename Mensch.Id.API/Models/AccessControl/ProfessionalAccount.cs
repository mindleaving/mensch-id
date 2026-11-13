using Newtonsoft.Json;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models.AccessControl
{
    public abstract class ProfessionalAccount : Account, IAccountWithPassword
    {
        public string Username { get; set; }
        [JsonIgnore]
        [TypescriptIsOptional]
        public string PasswordHash { get; set; }
    }
}
