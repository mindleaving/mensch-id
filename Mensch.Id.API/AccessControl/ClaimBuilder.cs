using System.Collections.Generic;
using System.Security.Claims;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.AccessControl
{
    public interface IClaimBuilder
    {
        List<Claim> BuildForLocalUser(LocalAnonymousAccount account);
        List<Claim> BuildForExternalLoginProvider(ExternalAccount account);
    }

    public class ClaimBuilder : IClaimBuilder
    {
        public List<Claim> BuildForLocalUser(
            LocalAnonymousAccount account)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, account.Id),
                new (MenschIdClaimTypes.AccountTypeClaimName, account.AccountType.ToString())
            };
            if (account.PersonId != null)
                claims.Add(new Claim(MenschIdClaimTypes.PersonIdClaimName, account.PersonId));
            return claims;
        }

        public List<Claim> BuildForExternalLoginProvider(
            ExternalAccount account)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, account.Id),
                new (MenschIdClaimTypes.AccountTypeClaimName, account.AccountType.ToString())
            };
            if(account.PersonId != null)
                claims.Add(new Claim(MenschIdClaimTypes.PersonIdClaimName, account.PersonId));
            //foreach (var externalClaim in externalClaims)
            //{
            //    claims.Add(externalClaim);
            //    //var transformedExternalClaim = new Claim(
            //    //    externalClaim.Type,
            //    //    externalClaim.Value,
            //    //    externalClaim.ValueType,
            //    //    Issuer,
            //    //    externalClaim.OriginalIssuer);
            //    //claims.Add(transformedExternalClaim);
            //}
            return claims;
        }
    }
}
