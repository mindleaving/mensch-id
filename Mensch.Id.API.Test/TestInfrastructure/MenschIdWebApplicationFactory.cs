using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Models.Shop;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow.Email;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Mensch.Id.API.Test.TestInfrastructure;

public class MenschIdWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestProviderId = "testprovider";
    public const string TestProviderDisplayName = "Test Provider";
    public const string TestProviderAuthority = "https://login.example.org";
    public const string TestProviderAuthorizationEndpoint = "https://login.example.org/authorize";
    public const string TestProviderClientId = "IntegrationTestClientId";
    public const string TestProviderClientSecret = "IntegrationTestClientSecretValue";
    public const string SecondProviderId = "secondprovider";
    public const string WebsiteBaseUrl = "https://mensch.example.org";

    private readonly string fileStoreDirectory = Path.Combine(Path.GetTempPath(), "mensch-id-tests", Guid.NewGuid().ToString());

    public InMemoryAccountStore AccountStore { get; } = new(TestKeys.CreateObscurer());
    public InMemoryIdStore IdStore { get; } = new();
    public InMemoryStore<Person> PersonStore { get; } = new();
    public InMemoryStore<AssignerControlledProfile> AssignerControlledProfileStore { get; } = new();
    public InMemoryStore<Verification> VerificationStore { get; } = new();
    public CapturingEmailSender EmailSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                var settings = new Dictionary<string, string>
                {
                    ["ExternalLogins:ObscureKey"] = TestKeys.ObscureKeyBase64,
                    ["Authentication:OpenIdConnect:Providers:0:Id"] = TestProviderId,
                    ["Authentication:OpenIdConnect:Providers:0:DisplayName"] = TestProviderDisplayName,
                    ["Authentication:OpenIdConnect:Providers:0:Authority"] = TestProviderAuthority,
                    ["Authentication:OpenIdConnect:Providers:0:ClientId"] = TestProviderClientId,
                    ["Authentication:OpenIdConnect:Providers:0:ClientSecret"] = TestProviderClientSecret,
                    ["Authentication:OpenIdConnect:Providers:1:Id"] = SecondProviderId,
                    ["Authentication:OpenIdConnect:Providers:1:DisplayName"] = "Second",
                    ["Authentication:OpenIdConnect:Providers:1:Authority"] = "https://second.example.org",
                    ["Authentication:OpenIdConnect:Providers:1:ClientId"] = "SecondClientId",
                    ["Authentication:OpenIdConnect:Providers:1:ClientSecret"] = "SecondClientSecretValue",
                    ["MongoDB:User"] = "test",
                    ["MongoDB:Password"] = "test",
                    ["FileStore:Directory"] = fileStoreDirectory,
                    ["CORS:Origins"] = WebsiteBaseUrl,
                    ["Email:WebsiteBaseUrl"] = WebsiteBaseUrl,
                    ["Email:UseDummySender"] = "true",
                    ["RateLimiter:TokenLimit"] = "10000",
                    ["RateLimiter:TokensPerPeriod"] = "10000"
                };
                configurationBuilder.AddInMemoryCollection(settings);
            });
        builder.ConfigureServices(ReplaceServices);
    }

    private void ReplaceServices(IServiceCollection services)
    {
        RemoveOrderDigestEmailSender(services);
        services.RemoveAll<IEmailSender>();
        services.AddSingleton<IEmailSender>(EmailSender);

        services.RemoveAll<IAccountStore>();
        services.AddSingleton<IAccountStore>(AccountStore);
        ReplaceTypeStore<Account>(services, AccountStore);
        services.RemoveAll<IIdStore>();
        services.AddSingleton<IIdStore>(IdStore);
        ReplaceTypeStore<MenschId>(services, IdStore);
        ReplaceTypeStore<Person>(services, PersonStore);
        ReplaceTypeStore<AssignerControlledProfile>(services, AssignerControlledProfileStore);
        ReplaceTypeStore<Verification>(services, VerificationStore);
        ReplaceTypeStore<AssignerAccountRequest>(services, new InMemoryStore<AssignerAccountRequest>());
        ReplaceTypeStore<MenschIdChallenge>(services, new InMemoryStore<MenschIdChallenge>());
        ReplaceTypeStore<Order>(services, new InMemoryStore<Order>());
        ReplaceTypeStore<Product>(services, new InMemoryStore<Product>());

        var schemeName = OidcProviderRegistry.GetSchemeName(new LoginProvider(TestProviderId));
        services.Configure<OpenIdConnectOptions>(
            schemeName,
            options =>
            {
                options.Configuration = new OpenIdConnectConfiguration
                {
                    Issuer = TestProviderAuthority,
                    AuthorizationEndpoint = TestProviderAuthorizationEndpoint,
                    TokenEndpoint = "https://login.example.org/token"
                };
            });
    }

    private static void RemoveOrderDigestEmailSender(IServiceCollection services)
    {
        var descriptors = services
            .Where(x => x.ImplementationType == typeof(OrderDigestEmailSender))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static void ReplaceTypeStore<T>(
        IServiceCollection services,
        IStore<T> store) where T : IId
    {
        services.RemoveAll<IStore<T>>();
        services.RemoveAll<IReadonlyStore<T>>();
        services.AddSingleton(store);
        services.AddSingleton<IReadonlyStore<T>>(store);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (Directory.Exists(fileStoreDirectory))
            Directory.Delete(fileStoreDirectory, true);
    }
}
