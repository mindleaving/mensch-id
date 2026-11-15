using System;
using System.Collections.Generic;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using NUnit.Framework;

namespace Mensch.Id.API.Test.AccessControl;

public class OidcProviderRegistryTest
{
    private static OidcProviderSettings CreateProvider(string id)
    {
        return new OidcProviderSettings
        {
            Id = id,
            DisplayName = id,
            Authority = $"https://{id}.example.org",
            ClientId = "client",
            ClientSecret = "secret"
        };
    }

    [Test]
    public void KnownProviderResolvesToScheme()
    {
        var registry = new OidcProviderRegistry([CreateProvider("google")]);

        var isFound = registry.TryGetSchemeName(new LoginProvider("google"), out var schemeName);

        Assert.That(isFound, Is.True);
        Assert.That(schemeName, Is.EqualTo("oidc-google"));
        Assert.That(registry.IsExternalProvider(new LoginProvider("google")), Is.True);
    }

    [Test]
    public void UnknownProviderIsNotResolved()
    {
        var registry = new OidcProviderRegistry([CreateProvider("google")]);

        var isFound = registry.TryGetSchemeName(new LoginProvider("facebook"), out var schemeName);

        Assert.That(isFound, Is.False);
        Assert.That(schemeName, Is.Null);
        Assert.That(registry.IsExternalProvider(LoginProvider.Local), Is.False);
    }

    [Test]
    public void SchemeNameRoundtripsToLoginProvider()
    {
        var loginProvider = new LoginProvider("microsoft");

        var schemeName = OidcProviderRegistry.GetSchemeName(loginProvider);
        var parsedLoginProvider = OidcProviderRegistry.GetLoginProvider(schemeName);

        Assert.That(parsedLoginProvider, Is.EqualTo(loginProvider));
    }

    [Test]
    public void NonOidcSchemeCannotBeParsed()
    {
        Assert.That(() => OidcProviderRegistry.GetLoginProvider("Cookies"), Throws.InvalidOperationException);
    }

    [Test]
    public void DuplicateProviderIdsAreRejected()
    {
        var providers = new List<OidcProviderSettings> { CreateProvider("google"), CreateProvider("google") };

        Assert.That(() => new OidcProviderRegistry(providers), Throws.ArgumentException);
    }

    [Test]
    public void ReservedLocalProviderIdIsRejected()
    {
        Assert.That(() => new OidcProviderRegistry([CreateProvider("local")]), Throws.ArgumentException);
    }

    [Test]
    public void InvalidProviderIdIsRejected()
    {
        Assert.That(() => new OidcProviderRegistry([CreateProvider("my-provider")]), Throws.ArgumentException);
    }

    [Test]
    public void ProviderWithoutAuthorityIsRejected()
    {
        var provider = CreateProvider("google");
        provider.Authority = null;

        Assert.That(() => new OidcProviderRegistry([provider]), Throws.ArgumentException);
    }

    [Test]
    public void EmptyProviderListIsAllowed()
    {
        var registry = new OidcProviderRegistry(Array.Empty<OidcProviderSettings>());

        Assert.That(registry.Providers, Is.Empty);
    }
}
