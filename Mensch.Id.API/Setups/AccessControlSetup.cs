using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Mensch.Id.API.Setups;

public class AccessControlSetup : ISetup
{
    public void Run(
        IServiceCollection services,
        IConfiguration configuration)
    {
        SetupExternalLoginObscurer(services, configuration);
        SetupDataProtection(services, configuration);
        SetupRedirectUrlValidator(services, configuration);

        services.AddSingleton<IClaimBuilder, ClaimBuilder>();
        var jwtPrivateKey = GetOrGenerateJwtPrivateKey(configuration);
        services.AddSingleton<ISecurityTokenBuilder>(_ => new JwtSecurityTokenBuilder(jwtPrivateKey));
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
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
            .AddCookie()
            .AddJwtBearer(
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = jwtPrivateKey,
                        ValidAlgorithms = new[] { "HS256" },
                        ValidTypes = new[] { "JWT" },
                        ValidAudience = JwtSecurityTokenBuilder.Audience,
                        ValidIssuer = JwtSecurityTokenBuilder.Issuer
                    };
                    // Events can be null
                    // ReSharper disable once ConstantNullCoalescingCondition
                    options.Events ??= new();
                    options.Events.OnMessageReceived = context => {

                        if (context.Request.Cookies.ContainsKey(JwtSecurityTokenBuilder.AccessTokenCookieName))
                        {
                            context.Token = context.Request.Cookies[JwtSecurityTokenBuilder.AccessTokenCookieName];
                        }

                        return Task.CompletedTask;
                    };
                });
        foreach (var oidcProvider in oidcProviders)
        {
            AddOpenIdConnectProvider(authenticationBuilder, oidcProvider);
        }

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
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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
        var options = new RedirectUrlValidatorOptions { AllowedOrigins = allowedOrigins.Distinct().ToList() };
        services.AddSingleton(new RedirectUrlValidator(options));
    }

    private static SymmetricSecurityKey GetOrGenerateJwtPrivateKey(IConfiguration configuration)
    {
        var privateKeyBase64 = configuration["Authentication:Jwt:PrivateKey"];
        if (!string.IsNullOrWhiteSpace(privateKeyBase64))
        {
            var privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
            return new SymmetricSecurityKey(privateKeyBytes);
        }
        var bytes = RandomNumberGenerator.GetBytes(32);
        Console.WriteLine("No JWT private key configured. A random key is used, which invalidates all sessions on restart. "
                          + "Store a base64 encoded 32-byte key as environment variable 'Authentication__Jwt__PrivateKey'.");
        return new SymmetricSecurityKey(bytes);
    }
}
