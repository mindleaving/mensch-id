﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Email;
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
        private readonly IAccountCreator accountCreator;
        private readonly IEmailSender emailSender;

        public AccountsController(
            IAccountStore store,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationModule authenticationModule,
            IAccountCreator accountCreator,
            IEmailSender emailSender)
        {
            this.store = store;
            this.httpContextAccessor = httpContextAccessor;
            this.authenticationModule = authenticationModule;
            this.accountCreator = accountCreator;
            this.emailSender = emailSender;
        }

        [Authorize]
        [AllowAnonymous]
        [HttpGet("is-logged-in")]
        public IActionResult GetIsLoggedIn()
        {
            if (IsLoggedIn())
            {
                var accountType = ControllerHelpers.GetAccountType(httpContextAccessor);
                return Ok(new IsLoggedInResponse(accountType!.Value));
            }
            return StatusCode((int)HttpStatusCode.Unauthorized);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyAccounts()
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var personId = ClaimsHelpers.GetPersonId(claims);
            if (personId == null)
            {
                var currentAccount = await store.GetFromClaimsAsync(claims);
                if(currentAccount is not PersonAccount personAccount || personAccount.PersonId == null)
                    return Ok(new List<Account>());
                personId = personAccount.PersonId;
            }
            var myAccounts = await store.GetAllForMenschIdAsync(personId);
            return Ok(myAccounts);
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
            account.EmailVerificationToken = EmailVerification.GenerateToken(account.EmailVerificationAndPasswordResetSalt, out var unencryptedToken);
            await store.StoreAsync(account);
            var verificationEmail = new VerificationEmail
            {
                AccountId = account.Id,
                RecipientAddress = account.Email,
                VerificationToken = unencryptedToken
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
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(accountId, out accountId))
                return BadRequest("Invalid account-ID");
            var account = await store.GetByIdAsync(accountId);
            if (account == null)
                return NotFound();
            if (account is not LocalAccount localAccount)
                return NotFound();
            if (localAccount.IsEmailVerified)
                return Ok();
            var decodedToken = HttpUtility.UrlDecode(token);
            if (!EmailVerification.Verify(decodedToken, localAccount))
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
            if (account is AdminAccount)
                return StatusCode((int)HttpStatusCode.Forbidden);
            account.PasswordResetToken = PasswordReset.GenerateToken(account.EmailVerificationAndPasswordResetSalt, out var unencryptedToken);
            await store.StoreAsync(account);
            var passwordResetEmail = new PasswordResetEmail
            {
                RecipientAddress = account.Email,
                AccountId = account.Id,
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
            if (accountId != null && accountId != body.AccountId)
                return BadRequest("Account ID of body doesn't match route");
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(body.AccountId, out accountId))
                return BadRequest("Invalid account-ID");
            var account = await store.GetByIdAsync(accountId);
            if (account == null)
                return NotFound();
            if (account is not LocalAccount localAccount)
                return NotFound();
            if (!PasswordReset.Verify(body.ResetToken, localAccount))
                return StatusCode((int)HttpStatusCode.Forbidden, "Invalid reset token");
            if(body.Password.Length < MinimumPasswordLength)
                return BadRequest($"Password too short. Must be at least {MinimumPasswordLength} characters long.");
            if (!await authenticationModule.ChangePasswordAsync(localAccount.Id, body.Password))
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unknown error occured");
            var authenticationResult = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation(localAccount.Email, body.Password));
            if (!authenticationResult.IsAuthenticated)
                return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult.Error);

            SetAccessTokenCookie(authenticationResult);
            return Ok(new IsLoggedInResponse(authenticationResult.AccountType!.Value));
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
                    personId = (currentAccount as PersonAccount)?.PersonId;
                }
            }
            switch (body.AccountType)
            {
                case AccountType.Local:
                {
                    if (!EmailValidator.IsValidEmailFormat(body.Email))
                        return BadRequest("Invalid email format");
                    var existingAccount = await store.GetLocalByEmailAsync(body.Email);
                    if (existingAccount != null)
                        return Conflict("You already have an account with this email. Use password reset to gain access if you have forgotten your password.");
                    var account = await accountCreator.CreateLocal(
                        body.Email,
                        body.Password,
                        body.PreferedLanguage ?? Language.en,
                        personId);
                    account.EmailVerificationToken = EmailVerification.GenerateToken(account.EmailVerificationAndPasswordResetSalt, out var unencryptedToken);
                    await store.StoreAsync(account);
                    var verificationEmail = new VerificationEmail
                    {
                        AccountId = account.Id,
                        RecipientAddress = account.Email,
                        VerificationToken = unencryptedToken
                    };
                    await emailSender.SendVerificationEmail(verificationEmail);
                    return Ok();
                }
                case AccountType.LocalAnonymous:
                {
                    var localAnonymousAccount = await accountCreator.CreateLocalAnonymous(
                        body.Password,
                        body.PreferedLanguage ?? Language.en,
                        personId);
                    var authenticationResult = authenticationModule.BuildSecurityTokenForUser(localAnonymousAccount);
                    SetAccessTokenCookie(authenticationResult);
                    return Ok(new IsLoggedInResponse(AccountType.LocalAnonymous));
                }
                case AccountType.External:
                    return BadRequest($"Invalid account type '{body.AccountType}'");
                case AccountType.Assigner:
                case AccountType.Admin:
                    return StatusCode((int)HttpStatusCode.Forbidden);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LocalLogin(
            [FromBody] LoginInformation loginInformation)
        {
            if (loginInformation == null)
                return BadRequest("No login information provided");
            var authenticationResult = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(loginInformation);
            if (authenticationResult.IsAuthenticated)
            {
                SetAccessTokenCookie(authenticationResult);
                return Ok(new IsLoggedInResponse(authenticationResult.AccountType!.Value));
            }
            return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult.Error);
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
        [HttpGet("convert-to-local")]
        public async Task<IActionResult> ConvertToLocal()
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
                    if (!authenticationResult.IsAuthenticated)
                        return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult.Error);
                    SetAccessTokenCookie(authenticationResult);
                    return Ok(new IsLoggedInResponse(authenticationResult.AccountType!.Value));
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
                    return BadRequest("Your JWT bearer token doesn't contain an account ID");
                var originalAccount = await store.GetByIdAsync(jwtAccountId);
                if(originalAccount is not PersonAccount personAccount || personAccount.PersonId == null)
                    return BadRequest("Your JWT bearer token doesn't contain a person ID. If you just created your profile please log out and back in again");
                personId = personAccount.PersonId;
            }

            var loginProviderClaims = claims.Except(jwtClaims).ToList();
            if(loginProviderClaims.Count == 0)
                return BadRequest("No external logins active");
            var account = await store.GetFromClaimsAsync(loginProviderClaims);
            if (account == null)
            {
                var externalLoginProviders = ClaimsHelpers.GetLoginProviders(loginProviderClaims)
                    .Except(new [] { LoginProvider.LocalJwt })
                    .ToList();
                if (externalLoginProviders.Count > 1)
                {
                    await HttpContext.SignOutAsync(); // Logout ASP.NET controlled external logins, keeps X-Access-Token-cookie intact.
                    return StatusCode(
                        (int)HttpStatusCode.Conflict,
                        "Multiple external logins are active. Cannot determine which of them to link. "
                        + "You have been logged out of all external logins.");
                }

                var externalLoginProvider = externalLoginProviders.Single();
                var externalId = ClaimsHelpers.GetExternalId(loginProviderClaims);
                account = await accountCreator.CreateExternal(externalLoginProvider, externalId);
            }

            if (account is not ExternalAccount externalAccount)
                return StatusCode((int)HttpStatusCode.InternalServerError, "Found unexpected account type");
            if (externalAccount.PersonId == personId)
                return Ok();
            if (externalAccount.PersonId != null)
            {
                await LogOutImpl();
                return StatusCode((int)HttpStatusCode.Forbidden, 
                    "That account is already linked to another profile. "
                    + "You have been logged out to avoid confusion about which account you are logged into.");
            }
            externalAccount.PersonId = personId;
            await store.StoreAsync(externalAccount);
            return Ok();
        }


        [Authorize]
        [HttpPost("link/local")]
        public async Task<IActionResult> LinkToLocalAccount(LoginInformation loginInformation)
        {
            if (loginInformation == null)
                return BadRequest("No login information provided");
            var authenticationResult = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(loginInformation);
            if (!authenticationResult.IsAuthenticated)
                return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult.Error);

            var accountsToBeLinked = (await store.GetLocalsByEmailMenschIdOrUsernameAsync(loginInformation.EmailMenschIdOrUsername))
                .OfType<PersonAccount>()
                .ToList();
            if(accountsToBeLinked.Any(account => account.PersonId != null))
                return StatusCode((int)HttpStatusCode.Forbidden, "One or more matching accounts are already linked to another profile");

            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var currentAccount = await store.GetFromClaimsAsync(claims);
            if (currentAccount is not PersonAccount currentPersonAccount)
                return StatusCode((int)HttpStatusCode.InternalServerError, "Couldn't find current account information");
            if (currentPersonAccount.PersonId == null)
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Your current account doesn't have a profile yet");

            foreach (var accountToBeLinked in accountsToBeLinked)
            {
                accountToBeLinked.PersonId = currentPersonAccount.PersonId;
                await store.StoreAsync(accountToBeLinked);
            }
            return Ok();
        }

        [Authorize]
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            if (!IsLoggedIn())
                return Ok();
            await LogOutImpl();
            return Ok();
        }

        private async Task LogOutImpl()
        {
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete(JwtSecurityTokenBuilder.AccessTokenCookieName);
        }

        [Authorize]
        [HttpPost("{accountId}/change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromRoute] string accountId,
            [FromBody] ChangePasswordRequest body)
        {
            if (body.AccountId != accountId)
                return BadRequest("Account ID in body doesn't match route");
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(body.AccountId, out accountId))
                return BadRequest("Invalid account-ID");
            if(body.NewPassword.Length < MinimumPasswordLength)
                return BadRequest($"Password too short. Must be at least {MinimumPasswordLength} characters long.");
            var authenticationResult = await authenticationModule.AuthenticateLocalByAccountIdAsync(accountId, body.CurrentPassword);
            if (!authenticationResult.IsAuthenticated)
            {
                switch (authenticationResult.Error)
                {
                    case AuthenticationErrorType.AuthenticationMethodNotAvailable:
                        return StatusCode((int)HttpStatusCode.ServiceUnavailable);
                    case AuthenticationErrorType.InvalidUserOrPassword:
                    case AuthenticationErrorType.Unknown:
                    case AuthenticationErrorType.EmailNotVerified:
                    case null:
                        return Unauthorized();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (authenticationResult.AccountType == AccountType.External)
                return BadRequest("Cannot change password for external account");
            if(await authenticationModule.ChangePasswordAsync(accountId, body.NewPassword))
                return Ok();
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [Authorize]
        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAccount(
            [FromRoute] string accountId)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(accountId, out accountId))
                return BadRequest("Invalid account-ID");
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var personId = ClaimsHelpers.GetPersonId(claims);
            var matchingAccount = await store.GetByIdAsync(accountId);
            if (matchingAccount == null)
                return Ok();
            if (matchingAccount is not PersonAccount personAccount || personAccount.PersonId == null)
                return BadRequest("Only accounts associated with a mensch.ID can be deleted");
            if (personAccount.PersonId != personId)
            {
                return StatusCode(
                    (int)HttpStatusCode.Forbidden, 
                    "Your current login indicates a different mensch.ID than the account you are trying to delete. "
                    + "If this is your account, please login using credentials corresponding to the mensch.ID associated with that account");
            }
            await store.DeleteAsync(accountId);
            return Ok();
        }


        private bool IsLoggedIn()
        {
            return httpContextAccessor.HttpContext?.User.Identities.Any(x => x.IsAuthenticated) ?? false;
        }

        private void SetAccessTokenCookie(
            AuthenticationResult authenticationResult)
        {
            if (!authenticationResult.IsAuthenticated)
                throw new InvalidOperationException("Cannot set access  token cookie, because authentication failed");
            Response.Cookies.Append(
                JwtSecurityTokenBuilder.AccessTokenCookieName, 
                authenticationResult.AccessToken, 
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.Add(JwtSecurityTokenBuilder.ExpirationTime)
                });
        }
    }
}
