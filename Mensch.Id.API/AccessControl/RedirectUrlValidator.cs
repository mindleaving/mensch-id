using System;
using System.Collections.Generic;
using System.Linq;

namespace Mensch.Id.API.AccessControl;

public class RedirectUrlValidatorOptions
{
    public List<string> AllowedOrigins { get; set; } = [];
}

public class RedirectUrlValidator
{
    private readonly HashSet<string> allowedOrigins;

    public RedirectUrlValidator(RedirectUrlValidatorOptions options)
    {
        var normalizedOrigins = options.AllowedOrigins
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().TrimEnd('/'));
        allowedOrigins = new HashSet<string>(normalizedOrigins, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsAllowed(string redirectUrl)
    {
        if (string.IsNullOrWhiteSpace(redirectUrl))
            return false;
        if (IsLocalPath(redirectUrl))
            return true;
        if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out var redirectUri))
            return false;
        if (redirectUri.Scheme != Uri.UriSchemeHttps)
            return false;
        var redirectOrigin = redirectUri.GetLeftPart(UriPartial.Authority);
        return allowedOrigins.Contains(redirectOrigin);
    }

    private static bool IsLocalPath(string redirectUrl)
    {
        if (redirectUrl[0] != '/')
            return false;
        if (redirectUrl.Length == 1)
            return true;
        return redirectUrl[1] != '/' && redirectUrl[1] != '\\';
    }
}
