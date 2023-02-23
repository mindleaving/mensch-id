using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.AccessControl
{
    public interface IAuthenticationModule
    {
        Task<bool> ChangePasswordAsync(string accountId, string password, bool changePasswordOnNextLogin = false);
        Task<AuthenticationResult> AuthenticateLocalByEmailOrMenschIdAsync(LoginInformation loginInformation);
        Task<AuthenticationResult> AuthenticateLocalByAccountIdAsync(string accountId, string password);
        Task<AuthenticationResult> AuthenticateExternalAsync(List<Claim> externalClaims);
        AuthenticationResult BuildSecurityTokenForUser(LocalAnonymousAccount localAnonymousAccount);
    }
}