using System.Collections.Generic;
using System.Security.Claims;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Helpers;

public class ClaimsHelpersTest
{
    private static readonly LoginProvider Google = new("google");

    private static List<Claim> CreateLocalClaims(
        string accountId,
        AccountType accountType)
    {
        return
        [
            new Claim(ClaimTypes.NameIdentifier, accountId, ClaimValueTypes.String, JwtSecurityTokenBuilder.Issuer),
            new Claim(MenschIdClaimTypes.AccountTypeClaimName, accountType.ToString(), ClaimValueTypes.String, JwtSecurityTokenBuilder.Issuer)
        ];
    }

    [Test]
    public void ExternalClaimsResolveToExternalProvider()
    {
        var claims = ClaimsHelpers.BuildExternalClaims(Google, "external-123");

        var loginProvider = ClaimsHelpers.GetLoginProvider(claims);
        var externalId = ClaimsHelpers.GetExternalId(claims);
        var accountType = ClaimsHelpers.GetAccountType(claims);

        Assert.That(loginProvider, Is.EqualTo(Google));
        Assert.That(externalId, Is.EqualTo("external-123"));
        Assert.That(accountType, Is.EqualTo(AccountType.External));
    }

    [Test]
    public void ExternalClaimsContainOnlySubjectAndProvider()
    {
        var claims = ClaimsHelpers.BuildExternalClaims(Google, "external-123");

        Assert.That(claims, Has.Count.EqualTo(2));
    }

    [Test]
    public void LocalClaimsResolveToLocalProvider()
    {
        var claims = CreateLocalClaims("account-1", AccountType.Assigner);

        var loginProvider = ClaimsHelpers.GetLoginProvider(claims);
        var accountId = ClaimsHelpers.GetExternalId(claims);
        var accountType = ClaimsHelpers.GetAccountType(claims);

        Assert.That(loginProvider, Is.EqualTo(LoginProvider.Local));
        Assert.That(accountId, Is.EqualTo("account-1"));
        Assert.That(accountType, Is.EqualTo(AccountType.Assigner));
    }

    [Test]
    public void ExternalProviderTakesPrecedenceOverLocalLogin()
    {
        var claims = CreateLocalClaims("account-1", AccountType.Local);
        var externalClaims = ClaimsHelpers.BuildExternalClaims(Google, "external-123");
        claims.AddRange(externalClaims);

        var loginProviders = ClaimsHelpers.GetLoginProviders(claims);
        var loginProvider = ClaimsHelpers.GetLoginProvider(claims);

        Assert.That(loginProviders, Is.EquivalentTo(new[] { LoginProvider.Local, Google }));
        Assert.That(loginProvider, Is.EqualTo(Google));
    }

    [Test]
    public void ProviderClaimFromForeignIssuerIsIgnored()
    {
        var claims = new List<Claim>
        {
            new(MenschIdClaimTypes.LoginProvider, "google", ClaimValueTypes.String, "oidc:microsoft"),
            new(MenschIdClaimTypes.Subject, "external-123", ClaimValueTypes.String, "oidc:microsoft")
        };

        var loginProviders = ClaimsHelpers.GetLoginProviders(claims);

        Assert.That(loginProviders, Is.Empty);
    }

    [Test]
    public void PersonIdIsOnlyReadFromLocalIssuer()
    {
        var claims = new List<Claim>
        {
            new(MenschIdClaimTypes.PersonIdClaimName, "20000101-ABCDE", ClaimValueTypes.String, "oidc:google")
        };

        var personId = ClaimsHelpers.GetPersonId(claims);

        Assert.That(personId, Is.Null);
    }

    [Test]
    public void EmptyClaimsThrow()
    {
        Assert.That(() => ClaimsHelpers.GetLoginProvider([]), Throws.Exception);
    }
}
