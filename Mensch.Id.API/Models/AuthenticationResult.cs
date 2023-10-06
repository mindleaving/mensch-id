using System.Collections.Generic;
using System.Security.Claims;
using Mensch.Id.API.Models.AccessControl;
using Newtonsoft.Json;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models
{
    public class AuthenticationResult
    {
        [JsonConstructor]
        private AuthenticationResult(
            bool isAuthenticated,
            AuthenticationErrorType? error,
            List<Claim> claims,
            string accessToken,
            AccountType? accountType)
        {
            IsAuthenticated = isAuthenticated;
            Error = error;
            Claims = claims;
            AccessToken = accessToken;
            AccountType = accountType;
        }

        public static AuthenticationResult Success(
            List<Claim> claims,
            string accessToken,
            AccountType accountType)
        {
            return new AuthenticationResult(true, null, claims, accessToken, accountType);
        }

        public static AuthenticationResult Failed(AuthenticationErrorType errorType)
        {
            return new AuthenticationResult(false, errorType, null, null, null);
        }

        public bool IsAuthenticated { get; }
        [TypescriptIsOptional]
        public List<Claim> Claims { get; set; }
        [TypescriptIsOptional]
        public string AccessToken { get; }
        public AccountType? AccountType { get; set; }
        public AuthenticationErrorType? Error { get; }
    }
}