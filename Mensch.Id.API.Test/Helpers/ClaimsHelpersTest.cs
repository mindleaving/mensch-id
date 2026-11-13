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

    [Test]
    public void AccountClaimsResolveAccountIdTypeAndPersonId()
    {
        var account = new LocalAccount { Id = "account-1", PersonId = "20000101-ACDEF" };
        var claims = new ClaimBuilder().Build(account);

        var accountId = ClaimsHelpers.GetAccountId(claims);
        var accountType = ClaimsHelpers.GetAccountType(claims);
        var personId = ClaimsHelpers.GetPersonId(claims);

        Assert.That(accountId, Is.EqualTo("account-1"));
        Assert.That(accountType, Is.EqualTo(AccountType.Local));
        Assert.That(personId, Is.EqualTo("20000101-ACDEF"));
    }

    [Test]
    public void AccountWithoutPersonHasNoPersonIdClaim()
    {
        var account = new AssignerAccount { Id = "assigner-1" };
        var claims = new ClaimBuilder().Build(account);

        Assert.That(ClaimsHelpers.GetPersonId(claims), Is.Null);
        Assert.That(ClaimsHelpers.GetAccountType(claims), Is.EqualTo(AccountType.Assigner));
    }

    [Test]
    public void ExternalClaimsResolveToExternalProviderButNotToAccount()
    {
        var claims = ClaimsHelpers.BuildExternalClaims(Google, "external-123");

        var loginProvider = ClaimsHelpers.GetExternalLoginProvider(claims);
        var externalId = ClaimsHelpers.GetExternalId(claims, Google);

        Assert.That(loginProvider, Is.EqualTo(Google));
        Assert.That(externalId, Is.EqualTo("external-123"));
        Assert.That(ClaimsHelpers.GetAccountId(claims), Is.Null);
        Assert.That(ClaimsHelpers.GetAccountType(claims), Is.Null);
        Assert.That(claims, Has.Count.EqualTo(2));
    }

    [Test]
    public void ProviderClaimFromForeignIssuerIsIgnored()
    {
        var claims = new List<Claim>
        {
            new(MenschIdClaimTypes.LoginProvider, "google", ClaimValueTypes.String, "oidc:microsoft"),
            new(MenschIdClaimTypes.Subject, "external-123", ClaimValueTypes.String, "oidc:microsoft")
        };

        var loginProvider = ClaimsHelpers.GetExternalLoginProvider(claims);

        Assert.That(loginProvider, Is.Null);
    }

    [Test]
    public void EmptyClaimsResolveToNothing()
    {
        Assert.That(ClaimsHelpers.GetAccountId([]), Is.Null);
        Assert.That(ClaimsHelpers.GetAccountType([]), Is.Null);
        Assert.That(ClaimsHelpers.GetExternalLoginProvider([]), Is.Null);
    }
}
