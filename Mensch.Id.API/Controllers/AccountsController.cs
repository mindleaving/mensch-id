using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Email;
using Mensch.Id.API.Workflow.Models;
using Mensch.Id.API.Workflow.ViewModelBuilders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Mensch.Id.API.Setups;
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
        private readonly IAccountTokenService accountTokenService;
        private readonly IOidcProviderRegistry oidcProviderRegistry;
        private readonly LoginProviderViewModelBuilder loginProviderViewModelBuilder;
        private readonly RedirectUrlValidator redirectUrlValidator;
        private readonly ISessionRevoker sessionRevoker;

        public AccountsController(
            IAccountStore store,
            IHttpContextAccessor httpContextAccessor,
            IAuthenticationModule authenticationModule,
            IAccountCreator accountCreator,
            IEmailSender emailSender,
            IAccountTokenService accountTokenService,
            IOidcProviderRegistry oidcProviderRegistry,
            LoginProviderViewModelBuilder loginProviderViewModelBuilder,
            RedirectUrlValidator redirectUrlValidator,
            ISessionRevoker sessionRevoker)
        {
            this.sessionRevoker = sessionRevoker;
            this.accountTokenService = accountTokenService;
            this.oidcProviderRegistry = oidcProviderRegistry;
            this.loginProviderViewModelBuilder = loginProviderViewModelBuilder;
            this.redirectUrlValidator = redirectUrlValidator;
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
        [HttpGet("is-recently-authenticated")]
        public async Task<IActionResult> GetIsRecentlyAuthenticated()
        {
            var isRecentlyAuthenticated = await RecentAuthentication.IsSessionRecentlyAuthenticated(HttpContext);
            return Ok(isRecentlyAuthenticated);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyAccounts()
        {
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var currentAccount = await store.GetFromClaimsAsync(claims);
            if (currentAccount is not PersonAccount { PersonId: not null } personAccount)
                return Ok(new List<Account>());
            var myAccounts = await store.GetAllForMenschIdAsync(personAccount.PersonId);
            return Ok(myAccounts);
        }


        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
        [HttpPost("resend-verification-email")]
        public async Task<IActionResult> ResendVerificationEmail(
            [FromBody] ResendVerificationBody body)
        {
            var account = await store.GetLocalByEmailAsync(body.Email);
            if (account == null || account.IsEmailVerified)
                return Ok();
            await SendVerificationEmail(account);
            return Ok();
        }

        private async Task SendVerificationEmail(LocalAccount account)
        {
            var verificationToken = accountTokenService.Generate(AccountTokenPurpose.EmailVerification, account.Id);
            account.EmailVerificationNonce = verificationToken.Nonce;
            await store.StoreAsync(account);
            var verificationEmail = new VerificationEmail
            {
                AccountId = account.Id,
                RecipientAddress = account.Email,
                VerificationToken = verificationToken.Token,
                PreferedLanguage = account.PreferedLanguage
            };
            await emailSender.SendVerificationEmail(verificationEmail);
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
            var isTokenValid = accountTokenService.Verify(
                AccountTokenPurpose.EmailVerification,
                token,
                localAccount.Id,
                localAccount.EmailVerificationNonce);
            if (!isTokenValid)
                return StatusCode((int)HttpStatusCode.Forbidden, "Invalid verification token");
            localAccount.IsEmailVerified = true;
            localAccount.EmailVerificationNonce = null;
            if (localAccount.PersonId == null && localAccount.PendingPersonId != null)
                localAccount.PersonId = localAccount.PendingPersonId;
            localAccount.PendingPersonId = null;
            await store.StoreAsync(localAccount);
            return Ok();
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordResetEmail(
            [FromBody] ResetPasswordRequest body)
        {
            var account = await store.GetLocalByEmailAsync(body.Email);
            if (account == null)
                return Ok();
            var resetToken = accountTokenService.Generate(AccountTokenPurpose.PasswordReset, account.Id);
            account.PasswordResetNonce = resetToken.Nonce;
            await store.StoreAsync(account);
            var passwordResetEmail = new PasswordResetEmail
            {
                RecipientAddress = account.Email,
                AccountId = account.Id,
                ResetToken = resetToken.Token
            };
            await emailSender.SendPasswordResetEmail(passwordResetEmail);
            return Ok();
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
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
            var isResetTokenValid = accountTokenService.Verify(
                AccountTokenPurpose.PasswordReset,
                body.ResetToken,
                localAccount.Id,
                localAccount.PasswordResetNonce);
            if (!isResetTokenValid)
                return StatusCode((int)HttpStatusCode.Forbidden, "Invalid reset token");
            if(body.Password.Length < MinimumPasswordLength)
                return BadRequest($"Password too short. Must be at least {MinimumPasswordLength} characters long.");
            if (localAccount.PendingPersonId != null)
            {
                localAccount.PendingPersonId = null;
                await store.StoreAsync(localAccount);
            }
            if (!await authenticationModule.ChangePasswordAsync(localAccount.Id, body.Password))
                return StatusCode((int)HttpStatusCode.InternalServerError, "An unknown error occured");
            await sessionRevoker.RevokeAll([localAccount.Id]);
            var loginInformation = new LoginInformation(localAccount.Email, body.Password);
            var authenticationResult = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(loginInformation);
            if (!authenticationResult.IsAuthenticated)
                return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult.Error);

            await SignInSession(authenticationResult);
            return Ok(new IsLoggedInResponse(authenticationResult.AccountType!.Value));
        }

        

        [Authorize]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
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
                var currentAccount = await store.GetFromClaimsAsync(claims);
                personId = (currentAccount as PersonAccount)?.PersonId;
            }
            if (personId != null && !await RecentAuthentication.IsSessionRecentlyAuthenticated(HttpContext))
                return StatusCode((int)HttpStatusCode.Forbidden, RecentAuthentication.ReauthenticationRequiredMessage);
            switch (body.AccountType)
            {
                case AccountType.Local:
                {
                    if (body.Email == null || !EmailValidator.IsValidEmailFormat(body.Email))
                        return BadRequest("Invalid email format");
                    var existingAccount = await store.GetLocalByEmailAsync(body.Email);
                    var isProfessionalUsername = await store.IsProfessionalUsernameInUse(body.Email);
                    if (existingAccount != null || isProfessionalUsername)
                        return Conflict("You already have an account with this email. Use password reset to gain access if you have forgotten your password.");
                    var account = await accountCreator.CreateLocal(
                        body.Email,
                        body.Password,
                        body.PreferedLanguage ?? Language.en,
                        personId);
                    await SendVerificationEmail(account);
                    return Ok();
                }
                case AccountType.LocalAnonymous:
                {
                    var localAnonymousAccount = await accountCreator.CreateLocalAnonymous(
                        body.Password,
                        body.PreferedLanguage ?? Language.en,
                        personId);
                    var authenticationResult = authenticationModule.BuildAuthenticationResult(localAnonymousAccount);
                    await SignInSession(authenticationResult);
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
        [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
        [HttpPost("login")]
        public async Task<IActionResult> LocalLogin(
            [FromBody] LoginInformation loginInformation)
        {
            if (loginInformation == null)
                return BadRequest("No login information provided");
            var authenticationResult = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(loginInformation);
            if (authenticationResult.IsAuthenticated)
            {
                await SignInSession(authenticationResult);
                return Ok(new IsLoggedInResponse(authenticationResult.AccountType!.Value));
            }
            return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult.Error);
        }

        [AllowAnonymous]
        [HttpGet("login-providers")]
        public IActionResult GetLoginProviders()
        {
            var loginProviders = loginProviderViewModelBuilder.BuildAll();
            return Ok(loginProviders);
        }

        [AllowAnonymous]
        [HttpGet("login/{loginProvider}")]
        [HttpPost("login/{loginProvider}")]
        public IActionResult ExternalLogin(
            [FromRoute] LoginProvider loginProvider,
            [FromQuery] string redirectUrl = null,
            [FromQuery] bool link = false)
        {
            if (!oidcProviderRegistry.TryGetSchemeName(loginProvider, out var schemeName))
                return BadRequest($"Invalid login provider '{loginProvider}'");
            if (!string.IsNullOrWhiteSpace(redirectUrl) && !redirectUrlValidator.IsAllowed(redirectUrl))
                return BadRequest("Invalid redirect URL");
            var callbackQuery = QueryString.Create(new Dictionary<string, string>
            {
                ["redirectUrl"] = string.IsNullOrWhiteSpace(redirectUrl) ? "/" : redirectUrl,
                ["link"] = link ? "true" : "false"
            });
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"{Request.PathBase}/api/accounts/external-login-callback{callbackQuery}"
            };
            return Challenge(properties, schemeName);
        }

        [AllowAnonymous]
        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(
            [FromQuery] string redirectUrl = "/",
            [FromQuery] bool link = false)
        {
            if (!redirectUrlValidator.IsAllowed(redirectUrl))
                return BadRequest("Invalid redirect URL");
            if (link)
                return Redirect(redirectUrl);
            var externalResult = await HttpContext.AuthenticateAsync(MenschIdAuthenticationSchemes.External);
            if (!externalResult.Succeeded)
                return Unauthorized();
            var externalClaims = externalResult.Principal!.Claims.ToList();
            var loginProvider = ClaimsHelpers.GetExternalLoginProvider(externalClaims);
            if (loginProvider == null || !oidcProviderRegistry.IsExternalProvider(loginProvider.Value))
                return Unauthorized();
            var externalId = ClaimsHelpers.GetExternalId(externalClaims, loginProvider.Value);
            if (string.IsNullOrEmpty(externalId))
                return Unauthorized();

            var existingAccount = await store.GetExternalByIdAsync(loginProvider.Value, externalId);
            if (existingAccount == null)
                await accountCreator.CreateExternal(loginProvider.Value, externalId);
            var authenticationResult = await authenticationModule.AuthenticateExternalAsync(loginProvider.Value, externalId);
            if (!authenticationResult.IsAuthenticated)
                return StatusCode((int)HttpStatusCode.Unauthorized, authenticationResult.Error);
            await HttpContext.SignOutAsync(MenschIdAuthenticationSchemes.External);
            await SignInSession(authenticationResult);
            return Redirect(redirectUrl);
        }


        [Authorize]
        [HttpPost("link/external")]
        public async Task<IActionResult> LinkToExternalAccount()
        {
            if (!await RecentAuthentication.IsSessionRecentlyAuthenticated(HttpContext))
                return StatusCode((int)HttpStatusCode.Forbidden, RecentAuthentication.ReauthenticationRequiredMessage);
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var currentAccount = await store.GetFromClaimsAsync(claims);
            if (currentAccount is not PersonAccount { PersonId: not null } currentPersonAccount)
                return BadRequest("Your current account doesn't have a profile yet");
            var personId = currentPersonAccount.PersonId;

            var externalResult = await HttpContext.AuthenticateAsync(MenschIdAuthenticationSchemes.External);
            if (!externalResult.Succeeded)
                return BadRequest("No external logins active");
            var externalClaims = externalResult.Principal!.Claims.ToList();
            await HttpContext.SignOutAsync(MenschIdAuthenticationSchemes.External);
            var externalLoginProvider = ClaimsHelpers.GetExternalLoginProvider(externalClaims);
            if (externalLoginProvider == null)
                return BadRequest("No external logins active");
            var externalId = ClaimsHelpers.GetExternalId(externalClaims, externalLoginProvider.Value);
            Account account = await store.GetExternalByIdAsync(externalLoginProvider.Value, externalId);
            if (account == null)
                account = await accountCreator.CreateExternal(externalLoginProvider.Value, externalId);

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
        [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
        [HttpPost("link/local")]
        public async Task<IActionResult> LinkToLocalAccount(LoginInformation loginInformation)
        {
            if (loginInformation == null)
                return BadRequest("No login information provided");
            if (!await RecentAuthentication.IsSessionRecentlyAuthenticated(HttpContext))
                return StatusCode((int)HttpStatusCode.Forbidden, RecentAuthentication.ReauthenticationRequiredMessage);
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

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            await LogOutImpl();
            return Ok();
        }

        private async Task LogOutImpl()
        {
            await HttpContext.SignOutAsync(MenschIdAuthenticationSchemes.Session);
            await HttpContext.SignOutAsync(MenschIdAuthenticationSchemes.External);
        }

        [Authorize]
        [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
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
                    case AuthenticationErrorType.AccountLocked:
                        return StatusCode((int)HttpStatusCode.Locked, authenticationResult.Error);
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
            if (!await authenticationModule.ChangePasswordAsync(accountId, body.NewPassword))
                return StatusCode((int)HttpStatusCode.InternalServerError);
            await sessionRevoker.RevokeAll([accountId]);
            var currentAccountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            if (currentAccountId == accountId && IsSessionAuthenticated())
                await SignInSession(authenticationResult);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAccount(
            [FromRoute] string accountId)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(accountId, out accountId))
                return BadRequest("Invalid account-ID");
            var claims = ControllerHelpers.GetClaims(httpContextAccessor);
            var currentAccount = await store.GetFromClaimsAsync(claims);
            var personId = (currentAccount as PersonAccount)?.PersonId;
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
            await sessionRevoker.RevokeAll([accountId]);
            return Ok();
        }


        private bool IsLoggedIn()
        {
            return httpContextAccessor.HttpContext?.User.Identities.Any(x => x.IsAuthenticated) ?? false;
        }

        private bool IsSessionAuthenticated()
        {
            return User.Identities.Any(x => x.IsAuthenticated && x.AuthenticationType == MenschIdAuthenticationSchemes.Session);
        }

        private async Task SignInSession(AuthenticationResult authenticationResult)
        {
            if (!authenticationResult.IsAuthenticated)
                throw new InvalidOperationException("Cannot sign in, because authentication failed");
            var properties = RecentAuthentication.CreateProperties(DateTimeOffset.UtcNow);
            await HttpContext.SignInAsync(
                MenschIdAuthenticationSchemes.Session,
                authenticationResult.Principal,
                properties);
        }
    }
}
