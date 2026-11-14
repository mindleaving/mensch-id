using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.ExternalServices;
using Mensch.Id.API.Setups;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Email;
using Mensch.Id.API.Workflow.ExternalServices;
using Mensch.Id.API.Workflow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Mensch.Id.API.Controllers;

[Authorize(Policy = AccountTypeRequirement.AdminPolicyName)]
[ApiController]
[Route("api/externalServices")]
public class ExternalServicesController : ControllerBase
{
    private readonly IStore<ExternalService, Guid> serviceStore;
    private readonly ExternalServiceClientRegistrar clientRegistrar;
    private readonly ExternalServiceAuthenticator serviceAuthenticator;
    private readonly IAccountTokenService accountTokenService;
    private readonly IEmailSender emailSender;
    private readonly IHttpContextAccessor httpContextAccessor;

    public ExternalServicesController(
        IStore<ExternalService, Guid> serviceStore,
        ExternalServiceClientRegistrar clientRegistrar,
        ExternalServiceAuthenticator serviceAuthenticator,
        IAccountTokenService accountTokenService,
        IEmailSender emailSender,
        IHttpContextAccessor httpContextAccessor)
    {
        this.serviceStore = serviceStore;
        this.clientRegistrar = clientRegistrar;
        this.serviceAuthenticator = serviceAuthenticator;
        this.accountTokenService = accountTokenService;
        this.emailSender = emailSender;
        this.httpContextAccessor = httpContextAccessor;
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] ExternalServiceRegistration body)
    {
        if (string.IsNullOrWhiteSpace(body.DisplayName))
            return BadRequest("Display name is missing");
        if (string.IsNullOrWhiteSpace(body.Email) || !EmailValidator.IsValidEmailFormat(body.Email))
            return BadRequest("A valid e-mail address is required");
        var service = new ExternalService
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            DisplayName = body.DisplayName.Trim(),
            Email = body.Email.Trim(),
            IsEmailVerified = false,
            ContactInformation = body.ContactInformation?.Trim(),
            Status = ExternalServiceStatus.Pending,
            CreatedTimestamp = DateTime.UtcNow
        };
        var clientSecret = await clientRegistrar.Register(service);
        await serviceStore.StoreAsync(service);
        await SendVerificationEmail(service);
        var response = new ExternalServiceRegistrationResponse
        {
            Id = service.Id,
            ClientId = service.ClientId,
            ClientSecret = clientSecret
        };
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/verify-email")]
    public async Task<IActionResult> VerifyEmail(
        [FromRoute] Guid id,
        [FromQuery] string token)
    {
        var service = await serviceStore.GetByIdAsync(id);
        if (service == null)
            return NotFound();
        if (service.IsEmailVerified)
            return Ok();
        var isTokenValid = accountTokenService.Verify(
            AccountTokenPurpose.ExternalServiceEmailVerification,
            token,
            service.Id.ToString(),
            service.EmailVerificationNonce);
        if (!isTokenValid)
            return StatusCode((int)HttpStatusCode.Forbidden, "Invalid verification token");
        service.IsEmailVerified = true;
        service.EmailVerificationNonce = null;
        await serviceStore.StoreAsync(service);
        return Ok();
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
    [HttpPost("{id:guid}/resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmail(
        [FromRoute] Guid id)
    {
        var service = await serviceStore.GetByIdAsync(id);
        if (service == null || service.IsEmailVerified)
            return Ok();
        await SendVerificationEmail(service);
        return Ok();
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.CredentialsPolicy)]
    [HttpPost("rotate-secret")]
    public async Task<IActionResult> RotateSecret(
        [FromBody] ExternalServiceCredentials body)
    {
        var service = await serviceAuthenticator.AuthenticateRegardlessOfStatus(body);
        if (service == null)
            return Unauthorized();
        var clientSecret = await clientRegistrar.RotateSecret(service);
        var response = new ExternalServiceSecretRotationResponse { ClientSecret = clientSecret };
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetServices(
        [FromQuery] string searchText = null,
        [FromQuery] ExternalServiceStatus? status = null)
    {
        var services = await serviceStore.GetAllAsync().ToListAsync();
        var matchingServices = services
            .Where(x => status == null || x.Status == status)
            .Where(x => MatchesSearchText(x, searchText))
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.CreatedTimestamp)
            .ToList();
        return Ok(matchingServices);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetService(
        [FromRoute] Guid id)
    {
        var service = await serviceStore.GetByIdAsync(id);
        if (service == null)
            return NotFound();
        return Ok(service);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id)
    {
        var service = await serviceStore.GetByIdAsync(id);
        if (service == null)
            return NotFound();
        if (!service.IsEmailVerified)
            return StatusCode((int)HttpStatusCode.Conflict, "The service's e-mail address has not been verified yet");
        if (service.Status == ExternalServiceStatus.Active)
            return Ok();
        await clientRegistrar.Enable(service);
        service.Status = ExternalServiceStatus.Active;
        service.ActivatedTimestamp = DateTime.UtcNow;
        service.ActivatedBy = ControllerHelpers.GetAccountId(httpContextAccessor);
        await serviceStore.StoreAsync(service);
        return Ok();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id)
    {
        var service = await serviceStore.GetByIdAsync(id);
        if (service == null)
            return NotFound();
        await clientRegistrar.Disable(service);
        service.Status = ExternalServiceStatus.Deactivated;
        await serviceStore.StoreAsync(service);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id)
    {
        var service = await serviceStore.GetByIdAsync(id);
        if (service == null)
            return Ok();
        await clientRegistrar.Unregister(service);
        await serviceStore.DeleteAsync(id);
        return Ok();
    }

    private async Task SendVerificationEmail(ExternalService service)
    {
        var verificationToken = accountTokenService.Generate(
            AccountTokenPurpose.ExternalServiceEmailVerification,
            service.Id.ToString());
        service.EmailVerificationNonce = verificationToken.Nonce;
        await serviceStore.StoreAsync(service);
        var email = new ExternalServiceVerificationEmail
        {
            RecipientAddress = service.Email,
            ServiceId = service.Id,
            ServiceName = service.DisplayName,
            VerificationToken = verificationToken.Token,
            PreferedLanguage = Language.en
        };
        await emailSender.SendExternalServiceVerificationEmail(email);
    }

    private static bool MatchesSearchText(
        ExternalService service,
        string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true;
        var trimmedSearchText = searchText.Trim();
        if (Guid.TryParse(trimmedSearchText, out var clientId))
            return service.ClientId == clientId;
        return service.DisplayName != null && service.DisplayName.Contains(trimmedSearchText, StringComparison.OrdinalIgnoreCase);
    }
}
