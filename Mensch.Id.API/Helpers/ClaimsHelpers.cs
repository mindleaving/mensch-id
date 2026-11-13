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

    public static string GetPersonId(List<Claim> claims)
    {
        var jwtClaims = claims.Where(x => x.Issuer == JwtSecurityTokenBuilder.Issuer).ToList();
        return jwtClaims.FirstOrDefault(x => x.Type == MenschIdClaimTypes.PersonIdClaimName)?.Value;
    }

    public static List<LoginProvider> GetLoginProviders(List<Claim> claims)
    {
        var loginProviders = new List<LoginProvider>();
        var hasLocalClaims = claims.Any(x => x.Issuer == JwtSecurityTokenBuilder.Issuer);
        if (hasLocalClaims)
            loginProviders.Add(LoginProvider.Local);
        var externalLoginProviders = claims
            .Where(x => x.Issuer != JwtSecurityTokenBuilder.Issuer && x.Type == MenschIdClaimTypes.LoginProvider)
            .Where(x => x.Issuer == ExternalIssuerPrefix + x.Value)
            .Select(x => new LoginProvider(x.Value))
            .Distinct();
        loginProviders.AddRange(externalLoginProviders);
        return loginProviders;
    }

    public static LoginProvider GetLoginProvider(List<Claim> claims)
    {
        if (claims.Count == 0)
            throw new Exception("No claims made");
        var loginProviders = GetLoginProviders(claims);
        var externalLoginProviders = loginProviders.Where(x => x != LoginProvider.Local).ToList();
        if (externalLoginProviders.Count > 0)
            return externalLoginProviders.Single();
        if (loginProviders.Contains(LoginProvider.Local))
            return LoginProvider.Local;
        throw new Exception("Could not determine login provider from claims");
    }

    public static string GetExternalId(List<Claim> claims)
    {
        var loginProvider = GetLoginProvider(claims);
        return GetExternalId(claims, loginProvider);
    }

    public static string GetExternalId(
        IEnumerable<Claim> claims,
        LoginProvider loginProvider)
    {
        if (loginProvider == LoginProvider.Local)
        {
            return claims
                .Single(x => x.Issuer == JwtSecurityTokenBuilder.Issuer && x.Type == ClaimTypes.NameIdentifier)
                .Value;
        }
        var issuer = GetExternalIssuer(loginProvider);
        return claims
            .Single(x => x.Issuer == issuer && x.Type == MenschIdClaimTypes.Subject)
            .Value;
    }

    public static AccountType? GetAccountType(List<Claim> claims)
    {
        var loginProvider = GetLoginProvider(claims);
        if (loginProvider != LoginProvider.Local)
            return AccountType.External;
        var accountTypeString = claims.Single(x => x.Type == MenschIdClaimTypes.AccountTypeClaimName).Value;
        return Enum.Parse<AccountType>(accountTypeString);
    }
}
