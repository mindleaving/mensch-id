using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Hubs;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Models.ExternalServices;
using Mensch.Id.API.Storage;
using Mensch.Id.API.ViewModels;
using Mensch.Id.API.Workflow.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers;

[ApiController]
[Route("api/service-challenges")]
public class ServiceChallengesController : ControllerBase
{
    public const int MaxPendingChallengesPerServiceAndPerson = 10;
    private const string ProfileIncompleteMessage = "You cannot answer challenges yet because you haven't finished your profile yet";
    private readonly IServiceChallengeStore challengeStore;
    private readonly IReadonlyStore<ExternalService, Guid> serviceStore;
    private readonly ExternalServiceAuthenticator serviceAuthenticator;
    private readonly ServiceChallengeResultTokenIssuer resultTokenIssuer;
    private readonly ServiceChallengeNotifier notifier;
    private readonly IAccountStore accountStore;
    private readonly IHttpContextAccessor httpContextAccessor;

    public ServiceChallengesController(
        IServiceChallengeStore challengeStore,
        IReadonlyStore<ExternalService, Guid> serviceStore,
        ExternalServiceAuthenticator serviceAuthenticator,
        ServiceChallengeResultTokenIssuer resultTokenIssuer,
        ServiceChallengeNotifier notifier,
        IAccountStore accountStore,
        IHttpContextAccessor httpContextAccessor)
    {
        this.challengeStore = challengeStore;
        this.serviceStore = serviceStore;
        this.serviceAuthenticator = serviceAuthenticator;
        this.resultTokenIssuer = resultTokenIssuer;
        this.notifier = notifier;
        this.accountStore = accountStore;
        this.httpContextAccessor = httpContextAccessor;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> CreateChallenge(
        [FromBody] ServiceChallengeCreationRequest body)
    {
        var service = await serviceAuthenticator.Authenticate(body);
        if (service == null)
            return Unauthorized();
        if (!MenschId.IsValid(body.MenschId))
            return BadRequest("Invalid mensch.ID");
        if (string.IsNullOrWhiteSpace(body.Description))
            return BadRequest("Description is missing");
        var menschId = MenschId.Parse(body.MenschId);
        var now = DateTime.UtcNow;
        var pendingChallengeCount = await CountPendingChallenges(
            service.Id,
            menschId.Value,
            now);
        if (pendingChallengeCount >= MaxPendingChallengesPerServiceAndPerson)
            return StatusCode((int)HttpStatusCode.TooManyRequests, "Too many pending challenges for this mensch.ID");
        var challenge = ServiceChallengeFactory.Create(
            service.Id,
            menschId.Value,
            body,
            now);
        await challengeStore.StoreAsync(challenge);
        var response = new ServiceChallengeCreationResponse
        {
            ChallengeId = challenge.Id,
            ExpiresAt = challenge.ExpiresAt,
            AppLink = ServiceChallengeFactory.BuildAppLink(challenge.Id)
        };
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("{challengeId}/result")]
    public async Task<IActionResult> GetResult(
        [FromRoute] string challengeId,
        [FromBody] ExternalServiceCredentials body)
    {
        var service = await serviceAuthenticator.Authenticate(body);
        if (service == null)
            return Unauthorized();
        if (!ServiceChallengeFactory.IsValidId(challengeId))
            return BadRequest("Invalid challenge-ID");
        var challenge = await challengeStore.GetByIdAsync(challengeId);
        if (challenge == null || challenge.ServiceId != service.Id)
            return NotFound();
        var now = DateTime.UtcNow;
        var result = BuildResult(challenge, now);
        return Ok(result);
    }

    [Authorize(Policy = RegularUserPolicy.PolicyName)]
    [HttpGet("{challengeId}")]
    public async Task<IActionResult> GetChallenge(
        [FromRoute] string challengeId)
    {
        if (!ServiceChallengeFactory.IsValidId(challengeId))
            return BadRequest("Invalid challenge-ID");
        var menschId = await GetMenschIdOfCurrentUser();
        if (menschId == null)
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, ProfileIncompleteMessage);
        var challenge = await challengeStore.GetByIdAsync(challengeId);
        if (challenge == null || challenge.MenschId != menschId)
            return NotFound();
        var service = await serviceStore.GetByIdAsync(challenge.ServiceId);
        if (service == null)
            return NotFound();
        var viewModel = BuildViewModel(challenge, service);
        return Ok(viewModel);
    }

    [Authorize(Policy = RegularUserPolicy.PolicyName)]
    [HttpPost("{challengeId}/accept")]
    public Task<IActionResult> AcceptChallenge(
        [FromRoute] string challengeId)
    {
        return AnswerChallenge(challengeId, ServiceChallengeState.Accepted);
    }

    [Authorize(Policy = RegularUserPolicy.PolicyName)]
    [HttpPost("{challengeId}/reject")]
    public Task<IActionResult> RejectChallenge(
        [FromRoute] string challengeId)
    {
        return AnswerChallenge(challengeId, ServiceChallengeState.Rejected);
    }

    private async Task<IActionResult> AnswerChallenge(
        string challengeId,
        ServiceChallengeState answer)
    {
        if (!ServiceChallengeFactory.IsValidId(challengeId))
            return BadRequest("Invalid challenge-ID");
        var menschId = await GetMenschIdOfCurrentUser();
        if (menschId == null)
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, ProfileIncompleteMessage);
        var challenge = await challengeStore.GetByIdAsync(challengeId);
        if (challenge == null || challenge.MenschId != menschId)
            return NotFound();
        var service = await serviceStore.GetByIdAsync(challenge.ServiceId);
        if (service == null || service.Status != ExternalServiceStatus.Active)
            return StatusCode((int)HttpStatusCode.Gone, "The service is no longer active");
        if (challenge.State != ServiceChallengeState.Pending)
            return StatusCode((int)HttpStatusCode.Conflict, "Challenge has already been answered");
        var now = DateTime.UtcNow;
        if (challenge.ExpiresAt <= now)
            return StatusCode((int)HttpStatusCode.Gone, "Challenge has expired");
        var resultToken = resultTokenIssuer.Issue(
            challenge,
            service,
            answer,
            now);
        var isAnswerStored = await challengeStore.TryAnswer(
            challenge.Id,
            menschId,
            answer,
            now,
            resultToken);
        if (!isAnswerStored)
            return StatusCode((int)HttpStatusCode.Conflict, "Challenge has already been answered or has expired");
        var result = new ServiceChallengeResult
        {
            ChallengeId = challenge.Id,
            Status = ServiceChallengeResultStatus.Answered,
            ResultToken = resultToken
        };
        await notifier.NotifyServiceAboutAnswer(service.Id, result);
        return Ok();
    }

    private async Task<string> GetMenschIdOfCurrentUser()
    {
        var claims = ControllerHelpers.GetClaims(httpContextAccessor);
        var account = await accountStore.GetFromClaimsAsync(claims) as PersonAccount;
        return account?.PersonId;
    }

    private async Task<int> CountPendingChallenges(
        Guid serviceId,
        string menschId,
        DateTime now)
    {
        var pendingChallenges = await challengeStore
            .SearchAsync(
                x => x.ServiceId == serviceId
                     && x.MenschId == menschId
                     && x.State == ServiceChallengeState.Pending
                     && x.ExpiresAt > now)
            .ToListAsync();
        return pendingChallenges.Count;
    }

    private async Task<Dictionary<Guid, ExternalService>> LoadServices(List<ServiceChallenge> challenges)
    {
        var serviceIds = challenges
            .Select(x => x.ServiceId)
            .Distinct()
            .ToList();
        var services = await serviceStore
            .SearchAsync(x => serviceIds.Contains(x.Id) && x.Status == ExternalServiceStatus.Active)
            .ToListAsync();
        return services.ToDictionary(x => x.Id);
    }

    private static ServiceChallengeResult BuildResult(
        ServiceChallenge challenge,
        DateTime now)
    {
        var status = GetResultStatus(challenge, now);
        return new ServiceChallengeResult
        {
            ChallengeId = challenge.Id,
            Status = status,
            ResultToken = status == ServiceChallengeResultStatus.Answered ? challenge.ResultToken : null
        };
    }

    private static ServiceChallengeResultStatus GetResultStatus(
        ServiceChallenge challenge,
        DateTime now)
    {
        if (challenge.State != ServiceChallengeState.Pending)
            return ServiceChallengeResultStatus.Answered;
        if (challenge.ExpiresAt <= now)
            return ServiceChallengeResultStatus.Expired;
        return ServiceChallengeResultStatus.Pending;
    }

    private static ServiceChallengeViewModel BuildViewModel(
        ServiceChallenge challenge,
        ExternalService service)
    {
        return new ServiceChallengeViewModel
        {
            Id = challenge.Id,
            ServiceDisplayName = service.DisplayName,
            ServiceContactInformation = service.ContactInformation,
            Description = challenge.Description,
            State = challenge.State,
            CreatedTimestamp = challenge.CreatedTimestamp,
            ExpiresAt = challenge.ExpiresAt
        };
    }
}
