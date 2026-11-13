using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenIddict.Abstractions;
using OpenIddict.MongoDb;
using OpenIddict.MongoDb.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Mensch.Id.API.Setups;

public class OpenIddictClientSettings
{
    public string ClientId { get; set; }
    public string DisplayName { get; set; }
    public List<string> RedirectUris { get; set; } = [];
    public List<string> PostLogoutRedirectUris { get; set; } = [];
}

public class OpenIddictSettings
{
    public const string AppSettingsSectionName = "OpenIddict";

    public string SigningKeyPath { get; set; }
    public string EncryptionKeyPath { get; set; }
    public List<OpenIddictClientSettings> Clients { get; set; } = [];
}

public class OpenIddictClientSeeder : IHostedService
{
    public const string ApiScope = "mensch_id_api";
    private readonly IServiceProvider serviceProvider;
    private readonly OpenIddictSettings settings;

    public OpenIddictClientSeeder(
        IServiceProvider serviceProvider,
        IOptions<OpenIddictSettings> settings)
    {
        this.serviceProvider = serviceProvider;
        this.settings = settings.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await CreateIndexes(scope.ServiceProvider, cancellationToken);
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        foreach (var client in settings.Clients)
        {
            var descriptor = BuildDescriptor(client);
            var existingApplication = await applicationManager.FindByClientIdAsync(client.ClientId, cancellationToken);
            if (existingApplication == null)
                await applicationManager.CreateAsync(descriptor, cancellationToken);
            else
                await applicationManager.UpdateAsync(existingApplication, descriptor, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static OpenIddictApplicationDescriptor BuildDescriptor(OpenIddictClientSettings client)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            DisplayName = client.DisplayName,
            ClientType = ClientTypes.Public,
            ConsentType = ConsentTypes.Implicit
        };
        foreach (var redirectUri in client.RedirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri));
        }
        foreach (var postLogoutRedirectUri in client.PostLogoutRedirectUris)
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutRedirectUri));
        }
        descriptor.Permissions.UnionWith(
        [
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,
            Permissions.Endpoints.Revocation,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.GrantTypes.RefreshToken,
            Permissions.ResponseTypes.Code,
            Permissions.Prefixes.Scope + ApiScope
        ]);
        descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
        return descriptor;
    }

    private static async Task CreateIndexes(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var context = serviceProvider.GetRequiredService<IOpenIddictMongoDbContext>();
        var options = serviceProvider.GetRequiredService<IOptionsMonitor<OpenIddictMongoDbOptions>>().CurrentValue;
        var database = await context.GetDatabaseAsync(cancellationToken);

        var applications = database.GetCollection<OpenIddictMongoDbApplication>(options.ApplicationsCollectionName);
        var clientIdIndex = new CreateIndexModel<OpenIddictMongoDbApplication>(
            Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(x => x.ClientId),
            new CreateIndexOptions { Unique = true });
        await applications.Indexes.CreateOneAsync(clientIdIndex, cancellationToken: cancellationToken);

        var scopes = database.GetCollection<OpenIddictMongoDbScope>(options.ScopesCollectionName);
        var scopeNameIndex = new CreateIndexModel<OpenIddictMongoDbScope>(
            Builders<OpenIddictMongoDbScope>.IndexKeys.Ascending(x => x.Name),
            new CreateIndexOptions { Unique = true });
        await scopes.Indexes.CreateOneAsync(scopeNameIndex, cancellationToken: cancellationToken);

        var authorizations = database.GetCollection<OpenIddictMongoDbAuthorization>(options.AuthorizationsCollectionName);
        var authorizationIndex = new CreateIndexModel<OpenIddictMongoDbAuthorization>(
            Builders<OpenIddictMongoDbAuthorization>.IndexKeys
                .Ascending(x => x.ApplicationId)
                .Ascending(x => x.Scopes)
                .Ascending(x => x.Status)
                .Ascending(x => x.Subject)
                .Ascending(x => x.Type));
        await authorizations.Indexes.CreateOneAsync(authorizationIndex, cancellationToken: cancellationToken);

        var tokens = database.GetCollection<OpenIddictMongoDbToken>(options.TokensCollectionName);
        var referenceIdIndex = new CreateIndexModel<OpenIddictMongoDbToken>(
            Builders<OpenIddictMongoDbToken>.IndexKeys.Ascending(x => x.ReferenceId),
            new CreateIndexOptions<OpenIddictMongoDbToken>
            {
                PartialFilterExpression = Builders<OpenIddictMongoDbToken>.Filter.Exists(x => x.ReferenceId),
                Unique = true
            });
        var tokenLookupIndex = new CreateIndexModel<OpenIddictMongoDbToken>(
            Builders<OpenIddictMongoDbToken>.IndexKeys
                .Ascending(x => x.ApplicationId)
                .Ascending(x => x.Status)
                .Ascending(x => x.Subject)
                .Ascending(x => x.Type));
        await tokens.Indexes.CreateManyAsync([referenceIdIndex, tokenLookupIndex], cancellationToken);
    }
}
