using System.Security.Claims;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;

namespace Mensch.Id.API.AccessControl;

public class AuthenticationResult
{
    private AuthenticationResult(
        bool isAuthenticated,
        AuthenticationErrorType? error,
        Account account,
        ClaimsPrincipal principal)
    {
        IsAuthenticated = isAuthenticated;
        Error = error;
        Account = account;
        Principal = principal;
    }

    public static AuthenticationResult Success(
        Account account,
        ClaimsPrincipal principal)
    {
        return new AuthenticationResult(true, null, account, principal);
    }

    public static AuthenticationResult Failed(AuthenticationErrorType errorType)
    {
        return new AuthenticationResult(false, errorType, null, null);
    }

    public bool IsAuthenticated { get; }
    public Account Account { get; }
    public ClaimsPrincipal Principal { get; }
    public AccountType? AccountType => Account?.AccountType;
    public AuthenticationErrorType? Error { get; }
}
