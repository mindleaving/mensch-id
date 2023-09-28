using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;

namespace Mensch.Id.API.Helpers
{
    public static class ClaimsHelpers
    {
        public static string GetPersonId(List<Claim> claims)
        {
            var jwtClaims = claims.Where(x => x.Issuer == JwtSecurityTokenBuilder.Issuer).ToList();
            return jwtClaims.FirstOrDefault(x => x.Type == MenschIdClaimTypes.PersonIdClaimName)?.Value;
        }

        public static LoginProvider GetLoginProvider(List<Claim> claims)
        {
            if(!claims.Any())
                throw new Exception("No claims made");
            var externalClaims = claims.Where(x => x.Issuer != JwtSecurityTokenBuilder.Issuer).ToList();
            if (externalClaims.Any())
            {
                var issuer = externalClaims.Select(x => x.Issuer).Distinct().Single();
                return issuer switch
                {
                    "Facebook" => LoginProvider.Facebook,
                    "Google" => LoginProvider.Google,
                    "Microsoft" => LoginProvider.Microsoft,
                    "Twitter" => LoginProvider.Twitter,
                    _ => throw new Exception($"Unknown claim issuer '{issuer}'")
                };
            }
            return LoginProvider.LocalJwt;
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
            var loginProviderClaims = claims.Where(x => x.Issuer == GetIssuer(loginProvider)).ToList();
            switch (loginProvider)
            {
                case LoginProvider.Unknown:
                    throw new Exception($"Invalid login provider '{loginProvider}'");
                case LoginProvider.Google:
                case LoginProvider.LocalJwt:
                case LoginProvider.Twitter:
                case LoginProvider.Microsoft:
                case LoginProvider.Facebook:
                    return loginProviderClaims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loginProvider), loginProvider, null);
            }
        }

        public static AccountType? GetAccountType(
            List<Claim> claims)
        {
            var loginProvider = GetLoginProvider(claims);
            switch (loginProvider)
            {
                case LoginProvider.Unknown:
                    throw new Exception($"Invalid login provider '{loginProvider}'");
                case LoginProvider.Google:
                case LoginProvider.Twitter:
                case LoginProvider.Facebook:
                case LoginProvider.Microsoft:
                    return AccountType.External;
                case LoginProvider.LocalJwt:
                    {
                        var accountTypeString = claims.Single(x => x.Type == MenschIdClaimTypes.AccountTypeClaimName).Value;
                        return Enum.Parse<AccountType>(accountTypeString);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(loginProvider), loginProvider, null);
            }
        }

        private static string GetIssuer(LoginProvider loginProvider)
        {
            switch (loginProvider)
            {
                case LoginProvider.Google:
                    return "Google";
                case LoginProvider.Twitter:
                    return "Twitter";
                case LoginProvider.Facebook:
                    return "Facebook";
                case LoginProvider.Microsoft:
                    return "Microsoft";
                case LoginProvider.LocalJwt:
                    return JwtSecurityTokenBuilder.Issuer;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loginProvider), loginProvider, null);
            }
        }
    }
}
