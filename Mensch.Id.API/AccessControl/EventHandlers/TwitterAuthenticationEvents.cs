using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication.Twitter;

namespace Mensch.Id.API.AccessControl.EventHandlers
{
    public class TwitterAuthenticationEvents : TwitterEvents
    {
        private readonly IAccountStore accountStore;
        private readonly IProfileCreator profileCreator;

        public TwitterAuthenticationEvents(
            IAccountStore accountStore,
            IProfileCreator profileCreator)
        {
            this.accountStore = accountStore;
            this.profileCreator = profileCreator;
        }

        public override async Task CreatingTicket(
            TwitterCreatingTicketContext context)
        {
            var loginProvider = LoginProvider.Twitter;
            var externalId = context.UserId;
            var existingAccount = await accountStore.GetExternalByIdAsync(loginProvider, externalId);
            if (existingAccount == null)
            {
                await profileCreator.CreateExternal(loginProvider, externalId);
            }
            await base.CreatingTicket(context);
        }
    }
}
