using System.Collections.Generic;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.ViewModels
{
    public class ProfileViewModel
    {
        public ProfileViewModel(
            string id,
            List<LoginProvider> loginProviders,
            List<Verification> verifications)
        {
            Id = id;
            LoginProviders = loginProviders;
            Verifications = verifications;
        }

        public string Id { get; }
        public List<LoginProvider> LoginProviders { get; }
        public List<Verification> Verifications { get; }
    }
}
