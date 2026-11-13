using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Mensch.Id.API.AccessControl;

public class OidcAuthenticationEvents : OpenIdConnectEvents
{
    private readonly IAccountStore accountStore;
    private readonly IAccountCreator accountCreator;

    public OidcAuthenticationEvents(
        IAccountStore accountStore,
        IAccountCreator accountCreator)
    {
        this.accountStore = accountStore;
        this.accountCreator = accountCreator;
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var loginProvider = OidcProviderRegistry.GetLoginProvider(context.Scheme.Name);
        var externalId = context.Principal?.Claims.FirstOrDefault(x => x.Type == MenschIdClaimTypes.Subject)?.Value;
        if (string.IsNullOrWhiteSpace(externalId))
        {
            context.Fail("ID token contains no subject");
            return;
        }
        var existingAccount = await accountStore.GetExternalByIdAsync(loginProvider, externalId);
        if (existingAccount == null)
            await accountCreator.CreateExternal(loginProvider, externalId);

        var minimalClaims = ClaimsHelpers.BuildExternalClaims(loginProvider, externalId);
        var identity = new ClaimsIdentity(minimalClaims, context.Scheme.Name);
        context.Principal = new ClaimsPrincipal(identity);
        await base.TokenValidated(context);
    }
}
