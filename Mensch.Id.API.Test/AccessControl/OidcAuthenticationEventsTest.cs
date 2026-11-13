using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;

namespace Mensch.Id.API.Test.AccessControl;

public class OidcAuthenticationEventsTest
{
    private static readonly LoginProvider Google = new("google");
    private InMemoryAccountStore accountStore;
    private OidcAuthenticationEvents events;

    [SetUp]
    public void Setup()
    {
        accountStore = new InMemoryAccountStore(TestKeys.CreateObscurer());
        events = new OidcAuthenticationEvents();
    }

    private static TokenValidatedContext CreateContext(params Claim[] claims)
    {
        var schemeName = OidcProviderRegistry.GetSchemeName(Google);
        var scheme = new AuthenticationScheme(schemeName, "Google", typeof(OpenIdConnectHandler));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "oidc"));
        return new TokenValidatedContext(
            new DefaultHttpContext(),
            scheme,
            new OpenIdConnectOptions(),
            principal,
            new AuthenticationProperties());
    }

    [Test]
    public async Task TokenValidationDoesNotCreateAccounts()
    {
        var context = CreateContext(new Claim("sub", "google-user-1"));

        await events.TokenValidated(context);

        Assert.That(accountStore.Items, Is.Empty);
    }

    [Test]
    public async Task OnlySubjectAndProviderClaimsAreKept()
    {
        var context = CreateContext(
            new Claim("sub", "google-user-1"),
            new Claim("email", "joe@example.org"),
            new Claim("name", "Joe Doe"));

        await events.TokenValidated(context);

        var claims = context.Principal!.Claims.ToList();
        Assert.That(claims, Has.Count.EqualTo(2));
        Assert.That(claims.Any(x => x.Value == "joe@example.org"), Is.False);
        Assert.That(claims.Any(x => x.Value == "Joe Doe"), Is.False);
        Assert.That(ClaimsHelpers.GetExternalLoginProvider(claims), Is.EqualTo(Google));
        Assert.That(ClaimsHelpers.GetExternalId(claims, Google), Is.EqualTo("google-user-1"));
        Assert.That(context.Principal.Identity!.AuthenticationType, Is.EqualTo(MenschIdAuthenticationSchemes.External));
    }

    [Test]
    public async Task TokenWithoutSubjectFails()
    {
        var context = CreateContext(new Claim("email", "joe@example.org"));

        await events.TokenValidated(context);

        Assert.That(context.Result, Is.Not.Null);
        Assert.That(context.Result.Failure, Is.Not.Null);
        Assert.That(accountStore.Items, Is.Empty);
    }
}
