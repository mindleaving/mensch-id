using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mensch.Id.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AuthorizationController : ControllerBase
{
    public const string ProviderParameterName = "provider";
    private readonly IAccountStore accountStore;
    private readonly IClaimBuilder claimBuilder;
    private readonly IOidcProviderRegistry oidcProviderRegistry;
    private readonly string websiteBaseUrl;

    public AuthorizationController(
        IAccountStore accountStore,
        IClaimBuilder claimBuilder,
        IOidcProviderRegistry oidcProviderRegistry,
        IConfiguration configuration)
    {
        this.accountStore = accountStore;
        this.claimBuilder = claimBuilder;
        this.oidcProviderRegistry = oidcProviderRegistry;
        websiteBaseUrl = (configuration["Email:WebsiteBaseUrl"] ?? string.Empty).TrimEnd('/');
    }

    [HttpGet("~/api/oidc/authorize")]
    [HttpPost("~/api/oidc/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved");
        var sessionResult = await HttpContext.AuthenticateAsync(MenschIdAuthenticationSchemes.Session);
        var account = sessionResult.Succeeded
            ? await GetAccount(sessionResult.Principal)
            : null;
        if (account == null)
            return RedirectToLogin(request);

        var principal = BuildOpenIddictPrincipal(account, request.GetScopes());
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/api/oidc/token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved");
        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            return ForbidWithError(Errors.UnsupportedGrantType, "The specified grant type is not supported");

        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var account = result.Succeeded
            ? await GetAccount(result.Principal)
            : null;
        if (account == null)
            return ForbidWithError(Errors.InvalidGrant, "The account no longer exists");

        var principal = BuildOpenIddictPrincipal(account, result.Principal!.GetScopes());
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/api/oidc/endsession")]
    [HttpPost("~/api/oidc/endsession")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> EndSession()
    {
        await HttpContext.SignOutAsync(MenschIdAuthenticationSchemes.Session);
        await HttpContext.SignOutAsync(MenschIdAuthenticationSchemes.External);
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return SignOut(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<Account> GetAccount(ClaimsPrincipal principal)
    {
        var accountId = principal?.GetClaim(Claims.Subject);
        if (string.IsNullOrEmpty(accountId))
            return null;
        return await accountStore.GetByIdAsync(accountId);
    }

    private IActionResult RedirectToLogin(OpenIddictRequest request)
    {
        if (request.HasPromptValue(PromptValues.None))
            return ForbidWithError(Errors.LoginRequired, "The user is not logged in");
        var authorizeUrl = BuildAuthorizeUrl(request);
        var encodedAuthorizeUrl = Uri.EscapeDataString(authorizeUrl);
        var provider = (string)request[ProviderParameterName];
        if (!string.IsNullOrEmpty(provider))
        {
            if (!LoginProvider.IsValid(provider) || !oidcProviderRegistry.IsExternalProvider(new LoginProvider(provider)))
                return ForbidWithError(Errors.InvalidRequest, "Unknown login provider");
            return Redirect($"{Request.PathBase}/api/accounts/login/{provider}?redirectUrl={encodedAuthorizeUrl}");
        }
        return Redirect($"{websiteBaseUrl}/login?returnUrl={encodedAuthorizeUrl}");
    }

    private string BuildAuthorizeUrl(OpenIddictRequest request)
    {
        var parameters = request.GetParameters()
            .Select(x => new KeyValuePair<string, string>(x.Key, (string)x.Value))
            .Where(x => x.Value != null);
        var queryString = QueryString.Create(parameters);
        return UriHelper.BuildAbsolute(
            Request.Scheme,
            Request.Host,
            Request.PathBase,
            Request.Path,
            queryString);
    }

    private ClaimsPrincipal BuildOpenIddictPrincipal(
        Account account,
        IEnumerable<string> scopes)
    {
        var principal = claimBuilder.BuildPrincipal(account, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        principal.SetScopes(scopes);
        principal.SetDestinations(GetDestinations);
        return principal;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        if (claim.Type == Claims.Subject)
            return [Destinations.AccessToken, Destinations.IdentityToken];
        if (claim.Type == MenschIdClaimTypes.AccountTypeClaimName || claim.Type == MenschIdClaimTypes.PersonIdClaimName)
            return [Destinations.AccessToken];
        return [];
    }

    private IActionResult ForbidWithError(
        string error,
        string errorDescription)
    {
        var properties = new AuthenticationProperties(new Dictionary<string, string>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = errorDescription
        });
        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
