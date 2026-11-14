using System;
using System.Net;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models.ExternalServices;
using Mensch.Id.API.Setups;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow.ExternalServices;
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
    private readonly IHttpContextAccessor httpContextAccessor;

    public ExternalServicesController(
        IStore<ExternalService, Guid> serviceStore,
        ExternalServiceClientRegistrar clientRegistrar,
        IHttpContextAccessor httpContextAccessor)
    {
        this.serviceStore = serviceStore;
        this.clientRegistrar = clientRegistrar;
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
        if (string.IsNullOrWhiteSpace(body.ContactInformation))
            return BadRequest("Contact information is missing");
        if (!IsValidRedirectUri(body.AcceptRedirectUri))
            return BadRequest("Accept redirect URI must be an absolute http(s) URI");
        if (!IsValidRedirectUri(body.RejectRedirectUri))
            return BadRequest("Reject redirect URI must be an absolute http(s) URI");
        var service = new ExternalService
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            DisplayName = body.DisplayName.Trim(),
            ContactInformation = body.ContactInformation.Trim(),
            AcceptRedirectUri = body.AcceptRedirectUri,
            RejectRedirectUri = body.RejectRedirectUri,
            Status = ExternalServiceStatus.Pending,
            CreatedTimestamp = DateTime.UtcNow
        };
        await serviceStore.StoreAsync(service);
        return Ok(service.Id);
    }

    [HttpGet]
    public IActionResult GetServices()
    {
        var services = serviceStore.GetAllAsync();
        return Ok(services);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        [FromRoute] Guid id)
    {
        var service = await serviceStore.GetByIdAsync(id);
        if (service == null)
            return NotFound();
        if (service.Status == ExternalServiceStatus.Active)
            return StatusCode((int)HttpStatusCode.Conflict, "Service is already active. Rotate its secret instead");
        var clientSecret = await clientRegistrar.RegisterWithNewSecret(service);
        service.Status = ExternalServiceStatus.Active;
        service.ActivatedTimestamp = DateTime.UtcNow;
        service.ActivatedBy = ControllerHelpers.GetAccountId(httpContextAccessor);
        await serviceStore.StoreAsync(service);
        var credentials = new ExternalServiceCredentials
        {
            ClientId = service.ClientId,
            ClientSecret = clientSecret
        };
        return Ok(credentials);
    }

    [HttpPost("{id:guid}/rotate-secret")]
    public async Task<IActionResult> RotateSecret(
        [FromRoute] Guid id)
    {
        var service = await serviceStore.GetByIdAsync(id);
        if (service == null)
            return NotFound();
        if (service.Status != ExternalServiceStatus.Active)
            return StatusCode((int)HttpStatusCode.Conflict, "Only active services can have their secret rotated");
        var clientSecret = await clientRegistrar.RegisterWithNewSecret(service);
        var credentials = new ExternalServiceCredentials
        {
            ClientId = service.ClientId,
            ClientSecret = clientSecret
        };
        return Ok(credentials);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        [FromRoute] Guid id)
    {
        var service = await serviceStore.GetByIdAsync(id);
        if (service == null)
            return NotFound();
        await clientRegistrar.Unregister(service);
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

    private static bool IsValidRedirectUri(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return false;
        return uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp;
    }
}
