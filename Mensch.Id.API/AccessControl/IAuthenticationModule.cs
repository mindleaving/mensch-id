using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.AccessControl
{
    public interface IAuthenticationModule
    {
        Task<bool> ChangePasswordAsync(string email, string password, bool changePasswordOnNextLogin = false);
        Task<AuthenticationResult> AuthenticateLocalAsync(LoginInformation loginInformation);
        Task<AuthenticationResult> AuthenticateExternalAsync(List<Claim> claims);
        AuthenticationResult BuildSecurityTokenForUser(LocalAnonymousAccount localAnonymousAccount);
    }
}