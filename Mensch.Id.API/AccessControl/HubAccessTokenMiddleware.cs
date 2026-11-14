using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Mensch.Id.API.AccessControl;

public class HubAccessTokenMiddleware
{
    public const string HubPathPrefix = "/hubs";
    private const string AccessTokenQueryParameterName = "access_token";
    private readonly RequestDelegate next;

    public HubAccessTokenMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var isHubRequest = context.Request.Path.StartsWithSegments(HubPathPrefix);
        var accessToken = context.Request.Query[AccessTokenQueryParameterName].ToString();
        var hasAuthorizationHeader = !string.IsNullOrEmpty(context.Request.Headers.Authorization);
        if (isHubRequest && !string.IsNullOrEmpty(accessToken) && !hasAuthorizationHeader)
            context.Request.Headers.Authorization = $"Bearer {accessToken}";
        return next(context);
    }
}
