using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mensch.Id.API.Setups;

public class AccessControlSetup : ISetup
{
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(14);
    public static readonly TimeSpan ExternalLoginLifetime = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public void Run(
        IServiceCollection services,
        IConfiguration configuration)
    {
        SetupExternalLoginObscurer(services, configuration);
        SetupDataProtection(services, configuration);
        SetupRedirectUrlValidator(services, configuration);

        services.AddSingleton<IClaimBuilder, ClaimBuilder>();
        services.AddScoped<IAuthenticationModule, AuthenticationModule>();
        services.AddScoped<IAccountCreator, AccountCreator>();
        services.AddSingleton<IPasswordHasher<IAccountWithPassword>, PasswordHasher<IAccountWithPassword>>();
        services.AddSingleton<IAccountTokenService, AccountTokenService>();
        services.AddScoped<OidcAuthenticationEvents>();

        var oidcProviders = configuration.GetSection(OidcProviderSettings.AppSettingsSectionName).Get<List<OidcProviderSettings>>() ?? [];
        var oidcProviderRegistry = new OidcProviderRegistry(oidcProviders);
        services.AddSingleton<IOidcProviderRegistry>(oidcProviderRegistry);

        var authenticationBuilder = services.AddAuthentication(
                options =>
                {
                    options.DefaultScheme = MenschIdAuthenticationSchemes.Default;
                    options.DefaultChallengeScheme = MenschIdAuthenticationSchemes.Default;
                })
            .AddPolicyScheme(
                MenschIdAuthenticationSchemes.Default,
                MenschIdAuthenticationSchemes.Default,
                options => options.ForwardDefaultSelector = SelectScheme)
            .AddCookie(MenschIdAuthenticationSchemes.Session, ConfigureSessionCookie)
            .AddCookie(MenschIdAuthenticationSchemes.External, ConfigureExternalCookie);
        foreach (var oidcProvider in oidcProviders)
        {
            AddOpenIdConnectProvider(authenticationBuilder, oidcProvider);
        }

        SetupOpenIddict(services, configuration);

        services.AddSingleton<IAuthorizationHandler, AccountTypeRequirementHandler>();
        services.AddSingleton<IAuthorizationHandler, RegularUserPolicy>();
        services.AddAuthorization(
            options =>
            {
                options.AddPolicy(AccountTypeRequirement.AssignerPolicyName, policy => policy.AddRequirements(new AccountTypeRequirement(AccountType.Assigner)));
                options.AddPolicy(AccountTypeRequirement.AdminPolicyName, policy => policy.AddRequirements(new AccountTypeRequirement(AccountType.Admin)));
                options.AddPolicy(RegularUserPolicy.PolicyName, policy => policy.AddRequirements(new RegularUserRequirement()));
            });
    }

    private static string SelectScheme(HttpContext context)
    {
        var authorizationHeader = context.Request.Headers.Authorization.ToString();
        if (authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        return MenschIdAuthenticationSchemes.Session;
    }

    private static void ConfigureSessionCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = MenschIdAuthenticationSchemes.SessionCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = SessionLifetime;
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    }

    private static void ConfigureExternalCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = MenschIdAuthenticationSchemes.ExternalCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = ExternalLoginLifetime;
        options.SlidingExpiration = false;
    }

    private static void SetupOpenIddict(
        IServiceCollection services,
        IConfiguration configuration)
    {
        const string OidcBasePath = "api/oidc/";
        var settingsSection = configuration.GetSection(OpenIddictSettings.AppSettingsSectionName);
        services.Configure<OpenIddictSettings>(settingsSection);
        var settings = settingsSection.Get<OpenIddictSettings>() ?? new OpenIddictSettings();
        var contentRoot = configuration[HostDefaults.ContentRootKey] ?? Directory.GetCurrentDirectory();
        var signingKeyPath = ResolveKeyPath(contentRoot, settings.SigningKeyPath, nameof(settings.SigningKeyPath));
        var encryptionKeyPath = ResolveKeyPath(contentRoot, settings.EncryptionKeyPath, nameof(settings.EncryptionKeyPath));
        var isTransportSecurityRequired = configuration.GetValue("OpenIddict:RequireHttps", true);

        services.AddOpenIddict()
            .AddCore(options => options.UseMongoDb())
            .AddServer(
                options =>
                {
                    options
                        .SetConfigurationEndpointUris(OidcBasePath + ".well-known/openid-configuration")
                        .SetJsonWebKeySetEndpointUris(OidcBasePath + ".well-known/jwks")
                    	.SetAuthorizationEndpointUris(OidcBasePath + "authorize")
                        .SetTokenEndpointUris(OidcBasePath + "token")
                        .SetEndSessionEndpointUris(OidcBasePath + "endsession")
                        .SetRevocationEndpointUris(OidcBasePath + "revoke");
                    options.AllowAuthorizationCodeFlow()
                        .AllowRefreshTokenFlow()
                        .RequireProofKeyForCodeExchange();
                	    options.Configure(o => o.CodeChallengeMethods.Remove(OpenIddictConstants.CodeChallengeMethods.Plain));
                    options.RegisterScopes(Scopes.OpenId, Scopes.OfflineAccess, OpenIddictClientSeeder.ApiScope);
                    options.SetAccessTokenLifetime(AccessTokenLifetime)
                        .SetRefreshTokenLifetime(RefreshTokenLifetime);
                    options.AddSigningKey(OidcServerKeyLoader.LoadSigningKey(signingKeyPath))
                        .AddEncryptionKey(OidcServerKeyLoader.LoadEncryptionKey(encryptionKeyPath));
                    var aspNetCoreBuilder = options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableEndSessionEndpointPassthrough();
                    if (!isTransportSecurityRequired)
                        aspNetCoreBuilder.DisableTransportSecurityRequirement();
                })
            .AddValidation(
                options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });
        services.AddHostedService<OpenIddictClientSeeder>();
    }

    private static string ResolveKeyPath(
        string contentRoot,
        string configuredPath,
        string settingName)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            throw new SecurityException($"No path configured for OpenIddict:{settingName}");
        return Path.GetFullPath(Path.Combine(contentRoot, configuredPath));
    }

    private static void AddOpenIdConnectProvider(
        AuthenticationBuilder authenticationBuilder,
        OidcProviderSettings provider)
    {
        var loginProvider = new LoginProvider(provider.Id);
        var schemeName = OidcProviderRegistry.GetSchemeName(loginProvider);
        authenticationBuilder.AddOpenIdConnect(
            schemeName,
            provider.DisplayName ?? provider.Id,
            options =>
            {
                options.SignInScheme = MenschIdAuthenticationSchemes.External;
                options.Authority = provider.Authority;
                options.ClientId = provider.ClientId;
                options.ClientSecret = provider.ClientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.Scope.Clear();
                options.Scope.Add(OpenIdConnectScope.OpenId);
                options.MapInboundClaims = false;
                options.GetClaimsFromUserInfoEndpoint = false;
                options.SaveTokens = false;
                options.CallbackPath = $"/api/accounts/external-login/{loginProvider.Value}";
                options.EventsType = typeof(OidcAuthenticationEvents);
            });
    }

    private static void SetupExternalLoginObscurer(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var obscureKeyBase64 = configuration["ExternalLogins:ObscureKey"];
        if (string.IsNullOrWhiteSpace(obscureKeyBase64))
            throw new SecurityException("No key provided for obscuring external logins (ExternalLogins:ObscureKey)");
        var obscureKey = Convert.FromBase64String(obscureKeyBase64);
        services.AddSingleton<IExternalLoginObscurer>(new ExternalLoginObscurer(obscureKey));
    }

    private static void SetupDataProtection(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var dataProtectionBuilder = services.AddDataProtection()
            .SetApplicationName("Mensch.Id.API");
        var fileStoreDirectory = configuration[$"{FileStoreSettings.AppSettingsSectionName}:Directory"];
        if (string.IsNullOrWhiteSpace(fileStoreDirectory))
            return;
        var keyDirectory = new DirectoryInfo(Path.Combine(fileStoreDirectory, "dataprotection-keys"));
        dataProtectionBuilder.PersistKeysToFileSystem(keyDirectory);
    }

    private static void SetupRedirectUrlValidator(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = new List<string>();
        var corsOrigins = configuration["CORS:Origins"];
        if (!string.IsNullOrWhiteSpace(corsOrigins))
            allowedOrigins.AddRange(corsOrigins.Split(','));
        var websiteBaseUrl = configuration["Email:WebsiteBaseUrl"];
        if (!string.IsNullOrWhiteSpace(websiteBaseUrl))
            allowedOrigins.Add(websiteBaseUrl);
        var apiBaseUrl = configuration["Authentication:ApiBaseUrl"];
        if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            allowedOrigins.Add(new Uri(apiBaseUrl).GetLeftPart(UriPartial.Authority));
        var options = new RedirectUrlValidatorOptions { AllowedOrigins = allowedOrigins.Distinct().ToList() };
        services.AddSingleton(new RedirectUrlValidator(options));
    }
}
