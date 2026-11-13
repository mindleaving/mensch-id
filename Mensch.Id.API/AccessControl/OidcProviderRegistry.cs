using System;
using System.Collections.Generic;
using System.Linq;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.AccessControl;

public class OidcProviderSettings
{
    public const string AppSettingsSectionName = "Authentication:OpenIdConnect:Providers";

    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Authority { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}

public interface IOidcProviderRegistry
{
    IReadOnlyList<OidcProviderSettings> Providers { get; }
    bool TryGetSchemeName(LoginProvider loginProvider, out string schemeName);
    bool IsExternalProvider(LoginProvider loginProvider);
}

public class OidcProviderRegistry : IOidcProviderRegistry
{
    private const string SchemePrefix = "oidc-";
    private readonly Dictionary<LoginProvider, OidcProviderSettings> providersById;

    public OidcProviderRegistry(IEnumerable<OidcProviderSettings> providers)
    {
        var providerList = providers.ToList();
        providersById = new Dictionary<LoginProvider, OidcProviderSettings>();
        foreach (var provider in providerList)
        {
            var loginProvider = new LoginProvider(provider.Id);
            if (loginProvider == LoginProvider.Local)
                throw new ArgumentException($"Login provider ID '{provider.Id}' is reserved");
            if (string.IsNullOrWhiteSpace(provider.Authority))
                throw new ArgumentException($"Login provider '{provider.Id}' has no authority");
            if (!providersById.TryAdd(loginProvider, provider))
                throw new ArgumentException($"Duplicate login provider ID '{provider.Id}'");
        }
        Providers = providerList;
    }

    public IReadOnlyList<OidcProviderSettings> Providers { get; }

    public static string GetSchemeName(LoginProvider loginProvider)
    {
        return SchemePrefix + loginProvider.Value;
    }

    public static LoginProvider GetLoginProvider(string schemeName)
    {
        if (schemeName == null || !schemeName.StartsWith(SchemePrefix, StringComparison.Ordinal))
            throw new InvalidOperationException($"Scheme '{schemeName}' is not an OpenID Connect login provider scheme");
        return new LoginProvider(schemeName[SchemePrefix.Length..]);
    }

    public bool TryGetSchemeName(
        LoginProvider loginProvider,
        out string schemeName)
    {
        if (!providersById.ContainsKey(loginProvider))
        {
            schemeName = null;
            return false;
        }
        schemeName = GetSchemeName(loginProvider);
        return true;
    }

    public bool IsExternalProvider(LoginProvider loginProvider)
    {
        return providersById.ContainsKey(loginProvider);
    }
}
