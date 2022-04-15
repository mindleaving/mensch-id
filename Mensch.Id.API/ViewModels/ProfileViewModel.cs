using System.Collections.Generic;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.ViewModels
{
    public class ProfileViewModel : Person
    {
        public List<LoginProvider> LoginProviders { get; set; }
        public List<Verification> Verifications { get; set; }
    }
}
