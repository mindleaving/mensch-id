using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;

namespace Mensch.Id.API.ViewModels
{
    public class AssignerAccountViewModel : IUserViewModel
    {
        public AssignerAccountViewModel(
            AssignerAccount account)
        {
            AccountId = account.Id;
            Name = account.Name;
            LogoUrl = account.LogoUrl;
            ContactInformation = account.Contact;
        }

        public UserType UserType => UserType.Assigner;
        public string AccountId { get; }
        public string Name { get; }
        public string LogoUrl { get; }
        public Contact ContactInformation { get; }
    }
}
