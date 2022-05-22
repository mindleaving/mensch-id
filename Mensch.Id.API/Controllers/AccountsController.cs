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
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
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
        private const int MinimumPasswordLength = 8;

        private readonly IAccountStore store;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IAuthenticationModule authenticationModule;
        private readonly IProfileCreator profileCreator;
        private readonly IEmailSender emailSender;

        public AccountsController(
            IAccountStore store,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationModule authenticationModule,
            IProfileCreator profileCreator,
            IEmailSender emailSender)
        {
            this.store = store;
            this.httpContextAccessor = httpContextAccessor;
            this.authenticationModule = authenticationModule;
            this.profileCreator = profileCreator;
            this.emailSender = emailSender;
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
        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmail(
            [FromBody] ResendVerificationBody body)
        {
            var account = await store.GetLocalByEmailAsync(body.Email);
            if (account == null)
                return NotFound();
            if (account.IsEmailVerified)
                return BadRequest("Email is already verified");
            account.EmailVerificationToken = EmailVerification.GenerateToken(account.Salt, out var unencryptedToken);
            await store.StoreAsync(account);
            var verificationEmail = new VerificationEmail
            {
                AccountId = account.Id,
                Email = account.Email,
                VerificationToken = unencryptedToken,
                Subject = GetTranslatedVerificationEmailSubject(account.PreferedLanguage),
                Message = GetTranslatedVerificationEmailMessage(account.PreferedLanguage)
            };
            await emailSender.SendVerificationEmail(verificationEmail);
            return Ok();
        }


        [AllowAnonymous]
        [HttpGet("{accountId}/verify-email")]
        public async Task<IActionResult> VerifyEmail(
            [FromRoute] string accountId,
            [FromQuery] string token)
        {
            var account = await store.GetByIdAsync(accountId);
            if (account == null)
                return NotFound();
            if (account is not LocalAccount localAccount)
                return NotFound();
            if (localAccount.IsEmailVerified)
                return Ok();
            if (!EmailVerification.Verify(token, localAccount))
                return StatusCode((int)HttpStatusCode.Forbidden, "Invalid verification token");
            localAccount.IsEmailVerified = true;
            localAccount.EmailVerificationToken = null;
            await store.StoreAsync(localAccount);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordResetEmail(
            [FromBody] ResetPasswordRequest body)
        {
            var account = await store.GetLocalByEmailAsync(body.Email);
            if (account == null)
                return NotFound();
            account.PasswordResetToken = PasswordReset.GenerateToken(account.Salt, out var unencryptedToken);
            await store.StoreAsync(account);
            var passwordResetEmail = new PasswordResetEmail
            {
                Email = account.Email,
                AccountId = account.Id,
                Subject = GetTranslatedPasswordResetSubject(account.PreferedLanguage),
                Message = GetTranslatedPasswordResetMessage(account.PreferedLanguage),
                ResetToken = unencryptedToken
            };
            await emailSender.SendPasswordResetEmail(passwordResetEmail);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("{accountId}/reset-password")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromRoute] string accountId,
            [FromBody] ResetPasswordBody body)
        {
            var account = await store.GetByIdAsync(body.AccountId);
            if (account == null)
                return NotFound();
            if (account is not LocalAccount localAccount)
                return NotFound();
            if (!PasswordReset.Verify(body.ResetToken, localAccount))
                return StatusCode((int)HttpStatusCode.Forbidden, "Invalid reset token");
            if(body.Password.Length < MinimumPasswordLength)
                return BadRequest($"Password too short. Must be at least {MinimumPasswordLength} characters long.");
            if (!await authenticationModule.ChangePasswordAsync(localAccount.Email, body.Password))
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unknown error occured");
            var authenticationResult = await authenticationModule.AuthenticateLocalAsync(new LoginInformation(localAccount.Email, body.Password));
            return Ok(authenticationResult);
        }

        private string GetTranslatedPasswordResetSubject(
            Language language)
        {
            return Translate(PasswordResetEmailContent.Subject, language);
        }

        private string GetTranslatedPasswordResetMessage(
            Language language)
        {
            return Translate(PasswordResetEmailContent.Message, language);
        }

        [Authorize]
        [AllowAnonymous]
        [HttpPost("register-local")]
        public async Task<IActionResult> RegisterLocalAccount(
            [FromBody] RegistrationInformation body)
        {
            if (body == null)
                return BadRequest("Missing body");
            if (body.Password.Length < MinimumPasswordLength)
                return BadRequest($"Password too short. Must be at least {MinimumPasswordLength} characters long.");
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            string personId = null;
            if (claims.Any())
            {
                personId = ClaimsHelpers.GetPersonId(claims);
                if (personId == null)
                {
                    var currentAccount = await store.GetFromClaimsAsync(claims);
                    personId = currentAccount?.PersonId;
                }
            }
            switch (body.AccountType)
            {
                case AccountType.Local:
                {
                    if (!EmailValidator.IsValidEmailFormat(body.Email))
                        return BadRequest("Invalid email format");
                    var account = await profileCreator.CreateLocal(
                        body.Email,
                        body.Password,
                        body.PreferedLanguage ?? Language.en,
                        personId);
                    account.EmailVerificationToken = EmailVerification.GenerateToken(account.Salt, out var unencryptedToken);
                    var verificationEmail = new VerificationEmail
                    {
                        AccountId = account.Id,
                        Email = account.Email,
                        Subject = GetTranslatedVerificationEmailSubject(account.PreferedLanguage),
                        Message = GetTranslatedVerificationEmailMessage(account.PreferedLanguage),
                        VerificationToken = unencryptedToken
                    };
                    await emailSender.SendVerificationEmail(verificationEmail);
                    return Ok();
                }
                case AccountType.LocalAnonymous:
                {
                    var localAnonymousAccount = await profileCreator.CreateLocalAnonymous(
                        body.Password,
                        body.PreferedLanguage ?? Language.en,
                        personId);
                    var authenticationResult = authenticationModule.BuildSecurityTokenForUser(localAnonymousAccount);
                    return Ok(authenticationResult);
                }
                case AccountType.External:
                    return BadRequest($"Invalid account type '{body.AccountType}'");
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult);
        }

        private string GetTranslatedVerificationEmailSubject(
            Language language)
        {
            return Translate(VerificationEmailContent.Subject, language);
        }

        private string GetTranslatedVerificationEmailMessage(
            Language language)
        {
            return Translate(VerificationEmailContent.Message, language);
        }

        private string Translate(
            IReadOnlyDictionary<Language, string> resourceDictionary,
            Language language)
        {
            if (resourceDictionary.ContainsKey(language))
                return resourceDictionary[language];
            return resourceDictionary[Language.en];
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
            var personId = ClaimsHelpers.GetPersonId(claims);
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
                return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult);

            var accountToBeLinked = await store.GetLocalByEmailOrMenschIdAsync(loginInformation.EmailOrMenschId);
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
