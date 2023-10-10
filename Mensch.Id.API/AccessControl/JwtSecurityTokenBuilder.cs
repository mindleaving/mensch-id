using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Mensch.Id.API.AccessControl
{
    public class JwtSecurityTokenBuilder : ISecurityTokenBuilder
    {
        private readonly SymmetricSecurityKey privateKey;

        public JwtSecurityTokenBuilder(
            SymmetricSecurityKey privateKey)
        {
            this.privateKey = privateKey;
        }

        public static readonly TimeSpan ExpirationTime = TimeSpan.FromHours(8);
        public const string Audience = "mensch.ID";
        public const string Issuer = "mensch.ID";
        public const string AccessTokenCookieName = "X-Access-Token";

        public string Build(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = Audience,
                Issuer = Issuer,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(ExpirationTime),
                SigningCredentials = new SigningCredentials(privateKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }
    }
}
