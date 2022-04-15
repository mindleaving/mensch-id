using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mensch.Id.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace Mensch.Id.API.AccessControl
{
    internal class JwtSecurityTokenBuilder : ISecurityTokenBuilder
    {
        private readonly SymmetricSecurityKey privateKey;
        private readonly TimeSpan expirationTime;

        public JwtSecurityTokenBuilder(
            SymmetricSecurityKey privateKey, 
            TimeSpan expirationTime)
        {
            this.privateKey = privateKey;
            this.expirationTime = expirationTime;
        }

        public const string PersonIdClaimName = "personId";
        public const string AccountTypeClaimName = "accountType";
        public const string Audience = "mensch.ID";
        public const string Issuer = "mensch.ID";

        public string BuildForLocalUser(LocalAccount account)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, account.Id),
                new (AccountTypeClaimName, account.AccountType.ToString())
            };
            if (account.PersonId != null)
                claims.Add(new Claim(PersonIdClaimName, account.PersonId));
            return BuildFromClaims(claims);
        }

        public string BuildForExternalLoginProvider(ExternalAccount account)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, account.Id),
                new (AccountTypeClaimName, account.AccountType.ToString())
            };
            if(account.PersonId != null)
                claims.Add(new Claim(PersonIdClaimName, account.PersonId));
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
            return BuildFromClaims(claims);
        }

        private string BuildFromClaims(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = Audience,
                Issuer = Issuer,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(expirationTime),
                SigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }
    }
}
