using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.ViewModels;

namespace Mensch.Id.API.Workflow.ViewModelBuilders
{
    public class ProfileViewModelBuilder
    {
        private readonly IReadonlyStore<Account> accountStore;
        private readonly IReadonlyStore<Verification> verificationStore;

        public ProfileViewModelBuilder(
            IReadonlyStore<Account> accountStore,
            IReadonlyStore<Verification> verificationStore)
        {
            this.accountStore = accountStore;
            this.verificationStore = verificationStore;
        }

        public async Task<ProfileViewModel> Build(
            Person model)
        {
            var accounts = await accountStore.SearchAsync(x => x.PersonId == model.Id);
            var loginProviders = accounts.OfType<ExternalAccount>()
                .Select(x => x.LoginProvider)
                .ToList();
            loginProviders.AddRange(accounts.OfType<LocalAccount>().Select(_ => LoginProvider.LocalJwt));
            loginProviders.AddRange(accounts.OfType<LocalAnonymousAccount>().Select(_ => LoginProvider.LocalJwt));
            var verifications = await verificationStore.SearchAsync(x => x.PersonId == model.Id);
            return new ProfileViewModel
            {
                LoginProviders = loginProviders,
                Verifications = verifications,
                Id = model.Id
            };
        }
    }
}
