using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;

namespace Mensch.Id.API.Helpers;

public static class ClaimsHelpers
{
    private const string ExternalIssuerPrefix = "oidc:";

    public static string GetExternalIssuer(LoginProvider loginProvider)
    {
        return ExternalIssuerPrefix + loginProvider.Value;
    }

    public static List<Claim> BuildExternalClaims(
        LoginProvider loginProvider,
        string externalId)
    {
        var issuer = GetExternalIssuer(loginProvider);
        return
        [
            new Claim(MenschIdClaimTypes.Subject, externalId, ClaimValueTypes.String, issuer),
            new Claim(MenschIdClaimTypes.LoginProvider, loginProvider.Value, ClaimValueTypes.String, issuer)
        ];
    }

    public static bool IsMenschIdAccountPrincipal(IEnumerable<Claim> claims)
    {
        return claims.Any(x => x.Type == MenschIdClaimTypes.AccountTypeClaimName);
    }

    public static string GetAccountId(IEnumerable<Claim> claims)
    {
        var claimList = claims.ToList();
        if (!IsMenschIdAccountPrincipal(claimList))
            return null;
        return claimList.FirstOrDefault(x => x.Type == MenschIdClaimTypes.Subject)?.Value
               ?? claimList.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
    }

    public static string GetPersonId(IEnumerable<Claim> claims)
    {
        return claims.FirstOrDefault(x => x.Type == MenschIdClaimTypes.PersonIdClaimName)?.Value;
    }

    public static AccountType? GetAccountType(IEnumerable<Claim> claims)
    {
        var accountTypeString = claims.FirstOrDefault(x => x.Type == MenschIdClaimTypes.AccountTypeClaimName)?.Value;
        if (accountTypeString == null)
            return null;
        return Enum.Parse<AccountType>(accountTypeString);
    }

    public static LoginProvider? GetExternalLoginProvider(IEnumerable<Claim> externalClaims)
    {
        var loginProviderClaim = externalClaims.FirstOrDefault(
            x => x.Type == MenschIdClaimTypes.LoginProvider && x.Issuer == ExternalIssuerPrefix + x.Value);
        if (loginProviderClaim == null || !LoginProvider.IsValid(loginProviderClaim.Value))
            return null;
        return new LoginProvider(loginProviderClaim.Value);
    }

    public static string GetExternalId(
        IEnumerable<Claim> externalClaims,
        LoginProvider loginProvider)
    {
        var issuer = GetExternalIssuer(loginProvider);
        return externalClaims
            .FirstOrDefault(x => x.Issuer == issuer && x.Type == MenschIdClaimTypes.Subject)
            ?.Value;
    }
}
