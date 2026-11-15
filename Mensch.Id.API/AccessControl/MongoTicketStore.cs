using System;
using System.Threading;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Mensch.Id.API.AccessControl;

public class MongoTicketStore : ITicketStore
{
    public const string CurrentWebSessionIdItemKey = "MenschId.CurrentWebSessionId";
    private const int MaxUserAgentLength = 500;
    private static readonly TimeSpan LastActivityUpdateInterval = TimeSpan.FromMinutes(1);
    private readonly TimeSpan defaultLifetime;

    public MongoTicketStore(TimeSpan defaultLifetime)
    {
        this.defaultLifetime = defaultLifetime;
    }

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        throw new InvalidOperationException("The HTTP context is required to store sessions");
    }

    public Task RenewAsync(
        string key,
        AuthenticationTicket ticket)
    {
        throw new InvalidOperationException("The HTTP context is required to renew sessions");
    }

    public Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        throw new InvalidOperationException("The HTTP context is required to retrieve sessions");
    }

    public Task RemoveAsync(string key)
    {
        throw new InvalidOperationException("The HTTP context is required to remove sessions");
    }

    public async Task<string> StoreAsync(
        AuthenticationTicket ticket,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var store = GetStore(httpContext);
        var now = DateTime.UtcNow;
        var session = new WebSession
        {
            Id = Guid.NewGuid(),
            AccountId = ClaimsHelpers.GetAccountId(ticket.Principal.Claims),
            SerializedTicket = TicketSerializer.Default.Serialize(ticket),
            UserAgent = GetUserAgent(httpContext),
            CreatedTimestamp = now,
            LastActivityTimestamp = now,
            ExpiresAt = GetExpiry(ticket, now)
        };
        await store.StoreAsync(session);
        return session.Id.ToString();
    }

    public async Task RenewAsync(
        string key,
        AuthenticationTicket ticket,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(key, out var sessionId))
            return;
        var store = GetStore(httpContext);
        var session = await store.GetByIdAsync(sessionId);
        if (session == null)
            return;
        var now = DateTime.UtcNow;
        session.SerializedTicket = TicketSerializer.Default.Serialize(ticket);
        session.LastActivityTimestamp = now;
        session.ExpiresAt = GetExpiry(ticket, now);
        await store.StoreAsync(session);
    }

    public async Task<AuthenticationTicket> RetrieveAsync(
        string key,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(key, out var sessionId))
            return null;
        var store = GetStore(httpContext);
        var session = await store.GetByIdAsync(sessionId);
        var now = DateTime.UtcNow;
        if (session == null || session.ExpiresAt <= now)
            return null;
        httpContext.Items[CurrentWebSessionIdItemKey] = session.Id;
        if (now - session.LastActivityTimestamp > LastActivityUpdateInterval)
        {
            session.LastActivityTimestamp = now;
            await store.StoreAsync(session);
        }
        return TicketSerializer.Default.Deserialize(session.SerializedTicket);
    }

    public async Task RemoveAsync(
        string key,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(key, out var sessionId))
            return;
        var store = GetStore(httpContext);
        await store.DeleteAsync(sessionId);
    }

    public static Guid? GetCurrentWebSessionId(HttpContext httpContext)
    {
        return httpContext.Items[CurrentWebSessionIdItemKey] as Guid?;
    }

    private static IStore<WebSession, Guid> GetStore(HttpContext httpContext)
    {
        return httpContext.RequestServices.GetRequiredService<IStore<WebSession, Guid>>();
    }

    private DateTime GetExpiry(
        AuthenticationTicket ticket,
        DateTime now)
    {
        return ticket.Properties.ExpiresUtc?.UtcDateTime ?? now.Add(defaultLifetime);
    }

    private static string GetUserAgent(HttpContext httpContext)
    {
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        return userAgent.Length > MaxUserAgentLength ? userAgent[..MaxUserAgentLength] : userAgent;
    }
}
