using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Mensch.Id.API.AccessControl;

public class OidcAuthenticationEvents : OpenIdConnectEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var loginProvider = OidcProviderRegistry.GetLoginProvider(context.Scheme.Name);
        var externalId = context.Principal?.Claims.FirstOrDefault(x => x.Type == MenschIdClaimTypes.Subject)?.Value;
        if (string.IsNullOrWhiteSpace(externalId))
        {
            context.Fail("ID token contains no subject");
            return;
        }
        var minimalClaims = ClaimsHelpers.BuildExternalClaims(loginProvider, externalId);
        var identity = new ClaimsIdentity(minimalClaims, MenschIdAuthenticationSchemes.External);
        context.Principal = new ClaimsPrincipal(identity);
        await base.TokenValidated(context);
    }
}
