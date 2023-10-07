using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.ViewModels
{
    public class AssignerAccountViewModel : IUserViewModel
    {
        public AssignerAccountViewModel(
            AssignerAccount account)
        {
            AccountId = account.Id;
            Name = account.Name;
            LogoId = account.LogoId;
            LogoImageType = account.LogoImageType;
            ContactInformation = account.Contact;
            PreferedLanguage = account.PreferedLanguage;
        }

        public UserType UserType => UserType.Assigner;
        public string AccountId { get; }
        public string Name { get; }
        [TypescriptIsOptional]
        public string LogoId { get; }
        [TypescriptIsOptional]
        public string LogoImageType { get; }
        public Contact ContactInformation { get; }
        public Language PreferedLanguage { get; set; }
    }
}
