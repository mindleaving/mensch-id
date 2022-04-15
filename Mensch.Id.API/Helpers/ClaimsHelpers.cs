using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Helpers
{
    public static class ClaimsHelpers
    {
        public static LoginProvider GetLoginProvider(List<Claim> claims)
        {
            var issuer = claims.Select(x => x.Issuer).Distinct().Single();
            return issuer switch
            {
                "Facebook" => LoginProvider.Facebook,
                "Google" => LoginProvider.Google,
                "Microsoft" => LoginProvider.Microsoft,
                "Twitter" => LoginProvider.Twitter,
                JwtSecurityTokenBuilder.Issuer => LoginProvider.LocalJwt,
                _ => throw new NotImplementedException()
            };
        }

        public static string GetExternalId(List<Claim> claims)
        {
            var loginProvider = GetLoginProvider(claims);
            return GetExternalId(claims, loginProvider);
        }

        public static string GetExternalId(
            List<Claim> claims,
            LoginProvider loginProvider)
        {
            var loginProviderClaims = claims.Where(x => x.Issuer == GetIssuer(loginProvider)).ToList();
            switch (loginProvider)
            {
                case LoginProvider.Unknown:
                    throw new Exception($"Invalid login provider '{loginProvider}'");
                case LoginProvider.Google:
                    break;
                case LoginProvider.LocalJwt:
                case LoginProvider.Twitter:
                case LoginProvider.Microsoft:
                    return loginProviderClaims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
                case LoginProvider.Facebook:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loginProvider), loginProvider, null);
            }

            throw new NotImplementedException();
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
