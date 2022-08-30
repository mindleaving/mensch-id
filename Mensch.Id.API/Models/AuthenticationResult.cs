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
            string accessToken,
            AccountType? accountType)
        {
            IsAuthenticated = isAuthenticated;
            Error = error;
            AccessToken = accessToken;
            AccountType = accountType;
        }

        public static AuthenticationResult Success(string accessToken, AccountType accountType)
        {
            return new AuthenticationResult(true, null, accessToken, accountType);
        }

        public static AuthenticationResult Failed(AuthenticationErrorType errorType)
        {
            return new AuthenticationResult(false, errorType, null, null);
        }

        public bool IsAuthenticated { get; }
        [TypescriptIsOptional]
        public string AccessToken { get; }
        public AccountType? AccountType { get; set; }
        public AuthenticationErrorType? Error { get; }
    }
}