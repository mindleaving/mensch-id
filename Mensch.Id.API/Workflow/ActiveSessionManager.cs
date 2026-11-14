using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.ViewModels;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mensch.Id.API.Workflow;

public class ActiveSessionManager : ISessionRevoker
{
    public const string UserAgentPropertyName = "user_agent";
    private readonly IOpenIddictAuthorizationManager authorizationManager;
    private readonly IOpenIddictTokenManager tokenManager;
    private readonly IOpenIddictApplicationManager applicationManager;
    private readonly IStore<WebSession, Guid> webSessionStore;

    public ActiveSessionManager(
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictTokenManager tokenManager,
        IOpenIddictApplicationManager applicationManager,
        IStore<WebSession, Guid> webSessionStore)
    {
        this.authorizationManager = authorizationManager;
        this.tokenManager = tokenManager;
        this.applicationManager = applicationManager;
        this.webSessionStore = webSessionStore;
    }

    public async Task<List<SessionViewModel>> GetSessions(
        IReadOnlyCollection<string> accountIds,
        Guid? currentWebSessionId,
        string currentAuthorizationId)
    {
        var appSessions = await GetAppSessions(accountIds, currentAuthorizationId);
        var webSessions = await GetWebSessions(accountIds, currentWebSessionId);
        return appSessions
            .Concat(webSessions)
            .OrderByDescending(x => x.LastActivityTimestamp ?? x.CreatedTimestamp)
            .ToList();
    }

    public async Task<bool> RevokeAppSession(
        IReadOnlyCollection<string> accountIds,
        string authorizationId)
    {
        var authorization = await authorizationManager.FindByIdAsync(authorizationId);
        if (authorization == null)
            return false;
        var subject = await authorizationManager.GetSubjectAsync(authorization);
        if (!accountIds.Contains(subject))
            return false;
        await RevokeAuthorization(authorization);
        return true;
    }

    public async Task<bool> RevokeWebSession(
        IReadOnlyCollection<string> accountIds,
        Guid webSessionId)
    {
        var session = await webSessionStore.GetByIdAsync(webSessionId);
        if (session == null || !accountIds.Contains(session.AccountId))
            return false;
        await webSessionStore.DeleteAsync(webSessionId);
        return true;
    }

    public async Task RevokeAll(IReadOnlyCollection<string> accountIds)
    {
        foreach (var accountId in accountIds)
        {
            var authorizations = await LoadAuthorizations(accountId);
            foreach (var authorization in authorizations)
            {
                await RevokeAuthorization(authorization);
            }
        }
        var webSessions = await LoadWebSessions(accountIds);
        foreach (var webSession in webSessions)
        {
            await webSessionStore.DeleteAsync(webSession.Id);
        }
    }

    private async Task RevokeAuthorization(object authorization)
    {
        var authorizationId = await authorizationManager.GetIdAsync(authorization);
        await authorizationManager.TryRevokeAsync(authorization);
        await tokenManager.RevokeByAuthorizationIdAsync(authorizationId);
    }

    private async Task<List<SessionViewModel>> GetAppSessions(
        IReadOnlyCollection<string> accountIds,
        string currentAuthorizationId)
    {
        var sessions = new List<SessionViewModel>();
        foreach (var accountId in accountIds)
        {
            var authorizations = await LoadAuthorizations(accountId);
            foreach (var authorization in authorizations)
            {
                var session = await BuildAppSession(authorization, currentAuthorizationId);
                if (session != null)
                    sessions.Add(session);
            }
        }
        return sessions;
    }

    private async Task<List<object>> LoadAuthorizations(string accountId)
    {
        var authorizations = new List<object>();
        await foreach (var authorization in authorizationManager.FindBySubjectAsync(accountId))
        {
            var isValid = await authorizationManager.HasStatusAsync(authorization, Statuses.Valid);
            if (isValid)
                authorizations.Add(authorization);
        }
        return authorizations;
    }

    private async Task<SessionViewModel> BuildAppSession(
        object authorization,
        string currentAuthorizationId)
    {
        var authorizationId = await authorizationManager.GetIdAsync(authorization);
        var lastActivity = await GetLastActivityOfActiveTokens(authorizationId);
        if (lastActivity == null)
            return null;
        var creationDate = await authorizationManager.GetCreationDateAsync(authorization);
        var clientName = await GetClientName(authorization);
        var properties = await authorizationManager.GetPropertiesAsync(authorization);
        return new SessionViewModel
        {
            Id = authorizationId,
            Type = SessionType.App,
            ClientName = clientName,
            UserAgent = GetUserAgent(properties),
            CreatedTimestamp = creationDate?.UtcDateTime,
            LastActivityTimestamp = lastActivity,
            IsCurrent = authorizationId == currentAuthorizationId
        };
    }

    private async Task<DateTime?> GetLastActivityOfActiveTokens(string authorizationId)
    {
        var now = DateTimeOffset.UtcNow;
        DateTime? lastActivity = null;
        var hasActiveToken = false;
        await foreach (var token in tokenManager.FindByAuthorizationIdAsync(authorizationId))
        {
            var creationDate = await tokenManager.GetCreationDateAsync(token);
            if (creationDate.HasValue && (lastActivity == null || creationDate.Value.UtcDateTime > lastActivity))
                lastActivity = creationDate.Value.UtcDateTime;
            var isValid = await tokenManager.HasStatusAsync(token, Statuses.Valid);
            var expirationDate = await tokenManager.GetExpirationDateAsync(token);
            if (isValid && (expirationDate == null || expirationDate > now))
                hasActiveToken = true;
        }
        return hasActiveToken ? lastActivity : null;
    }

    private async Task<string> GetClientName(object authorization)
    {
        var applicationId = await authorizationManager.GetApplicationIdAsync(authorization);
        if (string.IsNullOrEmpty(applicationId))
            return null;
        var application = await applicationManager.FindByIdAsync(applicationId);
        if (application == null)
            return null;
        return await applicationManager.GetDisplayNameAsync(application);
    }

    private static string GetUserAgent(IReadOnlyDictionary<string, JsonElement> properties)
    {
        if (!properties.TryGetValue(UserAgentPropertyName, out var userAgent))
            return null;
        return userAgent.ValueKind == JsonValueKind.String ? userAgent.GetString() : null;
    }

    private async Task<List<SessionViewModel>> GetWebSessions(
        IReadOnlyCollection<string> accountIds,
        Guid? currentWebSessionId)
    {
        var webSessions = await LoadWebSessions(accountIds);
        return webSessions
            .Select(x => new SessionViewModel
            {
                Id = x.Id.ToString(),
                Type = SessionType.Web,
                UserAgent = x.UserAgent,
                CreatedTimestamp = x.CreatedTimestamp,
                LastActivityTimestamp = x.LastActivityTimestamp,
                IsCurrent = x.Id == currentWebSessionId
            })
            .ToList();
    }

    private async Task<List<WebSession>> LoadWebSessions(IReadOnlyCollection<string> accountIds)
    {
        var now = DateTime.UtcNow;
        var accountIdList = accountIds.ToList();
        return await webSessionStore
            .SearchAsync(x => accountIdList.Contains(x.AccountId) && x.ExpiresAt > now)
            .ToListAsync();
    }
}
