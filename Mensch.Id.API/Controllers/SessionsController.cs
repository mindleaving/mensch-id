using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace Mensch.Id.API.Controllers;

[Authorize(Policy = RegularUserPolicy.PolicyName)]
[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ActiveSessionManager sessionManager;
    private readonly IAccountStore accountStore;
    private readonly IHttpContextAccessor httpContextAccessor;

    public SessionsController(
        ActiveSessionManager sessionManager,
        IAccountStore accountStore,
        IHttpContextAccessor httpContextAccessor)
    {
        this.sessionManager = sessionManager;
        this.accountStore = accountStore;
        this.httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions()
    {
        var accountIds = await GetAccountIdsOfCurrentPerson();
        var currentWebSessionId = MongoTicketStore.GetCurrentWebSessionId(HttpContext);
        var currentAuthorizationId = User.GetAuthorizationId();
        var sessions = await sessionManager.GetSessions(
            accountIds,
            currentWebSessionId,
            currentAuthorizationId);
        return Ok(sessions);
    }

    [HttpDelete("app/{authorizationId}")]
    public async Task<IActionResult> RevokeAppSession(
        [FromRoute] string authorizationId)
    {
        var accountIds = await GetAccountIdsOfCurrentPerson();
        var isRevoked = await sessionManager.RevokeAppSession(accountIds, authorizationId);
        return isRevoked ? Ok() : NotFound();
    }

    [HttpDelete("web/{webSessionId:guid}")]
    public async Task<IActionResult> RevokeWebSession(
        [FromRoute] Guid webSessionId)
    {
        var accountIds = await GetAccountIdsOfCurrentPerson();
        var isRevoked = await sessionManager.RevokeWebSession(accountIds, webSessionId);
        return isRevoked ? Ok() : NotFound();
    }

    [HttpPost("revoke-all")]
    public async Task<IActionResult> RevokeAllSessions()
    {
        var accountIds = await GetAccountIdsOfCurrentPerson();
        await sessionManager.RevokeAll(accountIds);
        await HttpContext.SignOutAsync(MenschIdAuthenticationSchemes.Session);
        return Ok();
    }

    private async Task<List<string>> GetAccountIdsOfCurrentPerson()
    {
        var claims = ControllerHelpers.GetClaims(httpContextAccessor);
        var currentAccount = await accountStore.GetFromClaimsAsync(claims);
        var personId = (currentAccount as PersonAccount)?.PersonId;
        if (personId == null)
            return [currentAccount.Id];
        var personAccounts = await accountStore.GetAllForMenschIdAsync(personId);
        return personAccounts
            .Select(x => x.Id)
            .Append(currentAccount.Id)
            .Distinct()
            .ToList();
    }
}
