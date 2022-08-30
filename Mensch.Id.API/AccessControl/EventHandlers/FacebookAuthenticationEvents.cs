using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Mensch.Id.API.AccessControl.EventHandlers
{
    public class FacebookAuthenticationEvents : OAuthEvents
    {
        private readonly IAccountStore accountStore;
        private readonly IAccountCreator accountCreator;

        public FacebookAuthenticationEvents(
            IAccountStore accountStore,
            IAccountCreator accountCreator)
        {
            this.accountStore = accountStore;
            this.accountCreator = accountCreator;
        }

        public override async Task CreatingTicket(
            OAuthCreatingTicketContext context)
        {
            var loginProvider = LoginProvider.Facebook;
            var externalId = ClaimsHelpers.GetExternalId(context.Identity.Claims.ToList());
            var existingAccount = await accountStore.GetExternalByIdAsync(loginProvider, externalId);
            if (existingAccount == null)
            {
                await accountCreator.CreateExternal(loginProvider, externalId);
            }
            await base.CreatingTicket(context);
        }
    }
}
