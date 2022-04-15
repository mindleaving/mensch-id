using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Mensch.Id.API.AccessControl.EventHandlers
{
    public class GoogleAuthenticationEvents : OAuthEvents
    {
        private readonly IAccountStore accountStore;
        private readonly IProfileCreator profileCreator;

        public GoogleAuthenticationEvents(
            IAccountStore accountStore,
            IProfileCreator profileCreator)
        {
            this.accountStore = accountStore;
            this.profileCreator = profileCreator;
        }

        public override async Task CreatingTicket(
            OAuthCreatingTicketContext context)
        {
            var loginProvider = LoginProvider.Google;
            var externalId = ClaimsHelpers.GetExternalId(context.Identity.Claims.ToList());
            var existingAccount = await accountStore.GetExternalByIdAsync(loginProvider, externalId);
            if (existingAccount == null)
            {
                await profileCreator.CreateExternal(loginProvider, externalId);
            }
            await base.CreatingTicket(context);
        }
    }
}
