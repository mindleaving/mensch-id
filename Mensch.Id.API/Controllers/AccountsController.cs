using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IAuthenticationModule = Mensch.Id.API.AccessControl.IAuthenticationModule;

namespace Mensch.Id.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountStore store;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAuthenticationModule authenticationModule;
        private readonly IProfileCreator profileCreator;

        public AccountsController(
            IAccountStore store,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationModule authenticationModule,
            IProfileCreator profileCreator)
        {
            this.store = store;
            this.httpContextAccessor = httpContextAccessor;
            this.authenticationModule = authenticationModule;
            this.profileCreator = profileCreator;
        }

        [Authorize]
        [AllowAnonymous]
        [HttpGet("is-logged-in")]
        public IActionResult GetIsLoggedIn()
        {
            if (IsLoggedIn())
                return Ok("1");
            return StatusCode((int)HttpStatusCode.Unauthorized, "0");
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LocalLogin([FromBody] LoginInformation loginInformation)
        {
            if (loginInformation == null)
                return BadRequest("No login information provided");
            var authenticationResult = await authenticationModule.AuthenticateLocalAsync(loginInformation);
            if (authenticationResult.IsAuthenticated)
                return Ok(authenticationResult);
            if (authenticationResult.Error == AuthenticationErrorType.UserNotFound && loginInformation.RegisterIfNotExists)
            {
                await profileCreator.CreateLocal(
                    loginInformation.Email,
                    loginInformation.Password);
                authenticationResult = await authenticationModule.AuthenticateLocalAsync(loginInformation);
                if (authenticationResult.IsAuthenticated)
                    return Ok(authenticationResult);
            }
            return Unauthorized();
        }

        [Authorize]
        [AllowAnonymous]
        [HttpGet("login/{loginProvider}")]
        [HttpPost("login/{loginProvider}")]
        public async Task<IActionResult> ExternalLogin(
            [FromRoute] LoginProvider loginProvider,
            [FromQuery] string redirectUrl = null)
        {
            if (IsLoggedIn())
            {
                var claims = ControllerHelpers.GetClaims(httpContextAccessor);
                var loginProviderInUse = ClaimsHelpers.GetLoginProvider(claims);
                if (loginProvider == loginProviderInUse)
                {
                    if (!string.IsNullOrWhiteSpace(redirectUrl))
                        return Redirect(redirectUrl);
                    return Ok();
                }
            }
            switch (loginProvider)
            {
                case LoginProvider.Google:
                    return Challenge(GoogleDefaults.AuthenticationScheme);
                case LoginProvider.Twitter:
                    return Challenge(TwitterDefaults.AuthenticationScheme);
                case LoginProvider.Facebook:
                    return Challenge(FacebookDefaults.AuthenticationScheme);
                case LoginProvider.Microsoft:
                    return Challenge(MicrosoftAccountDefaults.AuthenticationScheme);
                default:
                    return BadRequest($"Invalid login provider '{loginProvider}'");
            }
        }

        [Authorize]
        [HttpGet("accesstoken")]
        public async Task<IActionResult> GetAccessToken()
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var loginProvider = ClaimsHelpers.GetLoginProvider(claims);
            switch (loginProvider)
            {
                case LoginProvider.Google:
                case LoginProvider.Twitter:
                case LoginProvider.Facebook:
                case LoginProvider.Microsoft:
                    var authenticationResult = await authenticationModule.AuthenticateExternalAsync(claims);
                    return Ok(authenticationResult);
                case LoginProvider.LocalJwt:
                    return StatusCode((int)HttpStatusCode.Forbidden, "This method cannot be used with JWT authentication");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        [Authorize]
        [HttpPost("link/external")]
        public async Task<IActionResult> LinkToExternalAccount()
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var jwtClaims = claims.Where(x => x.Issuer == JwtSecurityTokenBuilder.Issuer).ToList();
            var personId = jwtClaims.FirstOrDefault(x => x.Type == JwtSecurityTokenBuilder.PersonIdClaimName)?.Value;
            if (personId == null)
            {
                var jwtAccountId = jwtClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                if (jwtAccountId == null)
                    return BadRequest("Your JWT bearer token doesn' contain an account ID");
                var originalAccount = await store.GetByIdAsync(jwtAccountId);
                if(originalAccount?.PersonId == null)
                    return BadRequest("Your JWT bearer token doesn't contain a person ID. If you just created your profile please log out and back in again");
                personId = originalAccount.PersonId;
            }

            var loginProviderClaims = claims.Except(jwtClaims).ToList();
            var account = await store.GetFromClaimsAsync(loginProviderClaims);
            if (account.PersonId == personId)
                return Ok();
            if (account.PersonId != null)
            {
                await httpContextAccessor.HttpContext.SignOutAsync();
                return StatusCode((int)HttpStatusCode.Forbidden, 
                    "You current account is already linked to another profile. "
                    + "You have been logged out to avoid confusion about which account you are logged into.");
            }
            account.PersonId = personId;
            await store.StoreAsync(account);
            return Ok();
        }

        [Authorize]
        [HttpPost("link/local")]
        public async Task<IActionResult> LinkToLocalAccount(LoginInformation loginInformation)
        {
            if (loginInformation == null)
                return BadRequest("No login information provided");
            var authenticationResult = await authenticationModule.AuthenticateLocalAsync(loginInformation);
            if (!authenticationResult.IsAuthenticated)
            {
                if (authenticationResult.Error == AuthenticationErrorType.UserNotFound && loginInformation.RegisterIfNotExists)
                {
                    await profileCreator.CreateLocal(
                        loginInformation.Email,
                        loginInformation.Password);
                    authenticationResult = await authenticationModule.AuthenticateLocalAsync(loginInformation);
                    if (!authenticationResult.IsAuthenticated)
                        return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult);
                }
            }

            var accountToBeLinked = await store.GetLocalByEmailAsync(loginInformation.Email);
            if(accountToBeLinked.PersonId != null)
                return StatusCode((int)HttpStatusCode.Forbidden, "That account is already linked to another profile");

            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var currentAccount = await store.GetFromClaimsAsync(claims);
            if (currentAccount == null)
                return StatusCode((int)HttpStatusCode.InternalServerError, "Couldn't find current account information");
            if (currentAccount.PersonId == null)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Your current account doesn't have a profile yet");

            accountToBeLinked.PersonId = currentAccount.PersonId;
            await store.StoreAsync(accountToBeLinked);
            return Ok();
        }

        [Authorize]
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            if (!IsLoggedIn())
                return Ok();
            await httpContextAccessor.HttpContext.SignOutAsync();
            return Ok();
        }



        private bool IsLoggedIn()
        {
            return httpContextAccessor.HttpContext?.User.Identities.Any(x => x.IsAuthenticated) ?? false;
        }
    }
}
