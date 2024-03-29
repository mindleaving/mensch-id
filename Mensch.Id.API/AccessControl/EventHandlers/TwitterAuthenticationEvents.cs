﻿using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication.Twitter;

namespace Mensch.Id.API.AccessControl.EventHandlers
{
    public class TwitterAuthenticationEvents : TwitterEvents
    {
        private readonly IAccountStore accountStore;
        private readonly IAccountCreator accountCreator;

        public TwitterAuthenticationEvents(
            IAccountStore accountStore,
            IAccountCreator accountCreator)
        {
            this.accountStore = accountStore;
            this.accountCreator = accountCreator;
        }

        public override async Task CreatingTicket(
            TwitterCreatingTicketContext context)
        {
            var loginProvider = LoginProvider.Twitter;
            var externalId = context.UserId;
            var existingAccount = await accountStore.GetExternalByIdAsync(loginProvider, externalId);
            if (existingAccount == null)
            {
                await accountCreator.CreateExternal(loginProvider, externalId);
            }
            await base.CreatingTicket(context);
        }
    }
}
