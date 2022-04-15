using Newtonsoft.Json;
using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models
{
    public class AuthenticationResult
    {
        [JsonConstructor]
        private AuthenticationResult(bool isAuthenticated,
            AuthenticationErrorType error,
            string accessToken)
        {
            IsAuthenticated = isAuthenticated;
            Error = error;
            AccessToken = accessToken;
        }

        public static AuthenticationResult Success(string accessToken)
        {
            return new AuthenticationResult(true, AuthenticationErrorType.Ok, accessToken);
        }

        public static AuthenticationResult Failed(AuthenticationErrorType errorType)
        {
            return new AuthenticationResult(false, errorType, null);
        }

        public bool IsAuthenticated { get; }
        [TypescriptIsOptional]
        public string AccessToken { get; }
        public AuthenticationErrorType Error { get; }
    }
}