using System;
using System.Collections.Generic;
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
        private readonly IStore<Person> personStore;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAuthenticationModule authenticationModule;
        private readonly IProfileCreator profileCreator;

        public AccountsController(
            IAccountStore store,
            IStore<Person> personStore,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationModule authenticationModule,
            IProfileCreator profileCreator)
        {
            this.store = store;
            this.personStore = personStore;
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
            if (authenticationResult.Error == AuthenticationErrorType.UserNotFound)
            {
                await profileCreator.CreateLocal(
                    loginInformation.Username,
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
                if (!string.IsNullOrWhiteSpace(redirectUrl))
                    return Redirect(redirectUrl);
                var claims = ControllerHelpers.GetClaims(httpContextAccessor);
                var authenticationResult = await ConstructAuthenticationResult(claims);
                return Ok(authenticationResult);
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

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpGet("accesstoken")]
        public async Task<IActionResult> GetAccessToken()
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var authenticationResult = await ConstructAuthenticationResult(claims);
            return Ok(authenticationResult);
        }


        private async Task<AuthenticationResult> ConstructAuthenticationResult(List<Claim> claims)
        {
            var loginProvider = ClaimsHelpers.GetLoginProvider(claims);
            if(loginProvider == LoginProvider.Unknown)
                return AuthenticationResult.Failed(AuthenticationErrorType.AuthenticationMethodNotAvailable);
            if (loginProvider == LoginProvider.LocalJwt)
                return AuthenticationResult.Success(null);
            return await authenticationModule.AuthenticateExternalAsync(claims);
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
            var user = httpContextAccessor.HttpContext?.User;
            if(user?.Identity == null)
            {
                return false;
            }
            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }
            return true;
        }
    }
}
