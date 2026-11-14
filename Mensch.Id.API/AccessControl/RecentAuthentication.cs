using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Mensch.Id.API.AccessControl;

public static class RecentAuthentication
{
    public const string AuthenticationTimeItemKey = "MenschId.AuthenticationTime";
    public const string ReauthenticationRequiredMessage = "ReauthenticationRequired";
    public static readonly TimeSpan MaximumAge = TimeSpan.FromMinutes(10);

    public static AuthenticationProperties CreateProperties(DateTimeOffset authenticationTime)
    {
        var properties = new AuthenticationProperties();
        properties.Items[AuthenticationTimeItemKey] = authenticationTime.ToString("O", CultureInfo.InvariantCulture);
        return properties;
    }

    public static async Task<AuthenticationProperties> GetCurrentSessionProperties(HttpContext httpContext)
    {
        var sessionResult = await httpContext.AuthenticateAsync(MenschIdAuthenticationSchemes.Session);
        if (!sessionResult.Succeeded || sessionResult.Properties == null)
            return new AuthenticationProperties();
        return sessionResult.Properties;
    }

    public static async Task<bool> IsSessionRecentlyAuthenticated(HttpContext httpContext)
    {
        var sessionResult = await httpContext.AuthenticateAsync(MenschIdAuthenticationSchemes.Session);
        if (!sessionResult.Succeeded || sessionResult.Properties == null)
            return false;
        var authenticationTime = GetAuthenticationTime(sessionResult.Properties);
        if (authenticationTime == null)
            return false;
        var age = DateTimeOffset.UtcNow - authenticationTime.Value;
        return age <= MaximumAge;
    }

    private static DateTimeOffset? GetAuthenticationTime(AuthenticationProperties properties)
    {
        if (!properties.Items.TryGetValue(AuthenticationTimeItemKey, out var authenticationTimeString))
            return null;
        var isParsed = DateTimeOffset.TryParse(
            authenticationTimeString,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var authenticationTime);
        return isParsed ? authenticationTime : null;
    }
}
