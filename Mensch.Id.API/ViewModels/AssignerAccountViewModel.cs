using Mensch.Id.API.Models;

namespace Mensch.Id.API.ViewModels
{
    public class AssignerAccountViewModel
    {
        public AssignerAccountViewModel(
            AssignerAccount account)
        {
            Name = account.Name;
            LogoUrl = account.LogoUrl;
        }

        public string Name { get; set; }
        public string LogoUrl { get; set; }
    }
}
