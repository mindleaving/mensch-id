﻿using System.Collections.Generic;
using System.Security.Claims;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;

namespace Mensch.Id.API.AccessControl
{
    public interface IClaimBuilder
    {
        List<Claim> BuildForLocalUser(LocalAnonymousAccount account);
        List<Claim> BuildForExternalLoginProvider(ExternalAccount account);
        List<Claim> BuildForProfessionalUser(ProfessionalAccount account);
    }

    public class ClaimBuilder : IClaimBuilder
    {
        public List<Claim> BuildForLocalUser(
            LocalAnonymousAccount account)
        {
            var claims = GetCommonClaims(account);
            if (account.PersonId != null)
                claims.Add(new Claim(MenschIdClaimTypes.PersonIdClaimName, account.PersonId));
            return claims;
        }

        public List<Claim> BuildForExternalLoginProvider(
            ExternalAccount account)
        {
            var claims = GetCommonClaims(account);
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

        public List<Claim> BuildForProfessionalUser(
            ProfessionalAccount account)
        {
            var claims = GetCommonClaims(account);
            return claims;
        }

        private static List<Claim> GetCommonClaims(
            Account account)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.Id),
                new(MenschIdClaimTypes.AccountTypeClaimName, account.AccountType.ToString())
            };
            return claims;
        }
    }
}
