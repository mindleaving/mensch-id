using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using IAuthenticationModule = Mensch.Id.API.AccessControl.IAuthenticationModule;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Setups;
using Mensch.Id.API.Test.TestInfrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Integration;

public class SecurityHardeningIntegrationTest
{
    private const string Password = "Password123!";
    private MenschIdWebApplicationFactory factory;

    [SetUp]
    public void Setup()
    {
        factory = new MenschIdWebApplicationFactory();
    }

    [TearDown]
    public void TearDown()
    {
        factory.Dispose();
    }

    private HttpClient CreateClient()
    {
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri(MenschIdWebApplicationFactory.ApiBaseUrl),
            AllowAutoRedirect = false,
            HandleCookies = true
        };
        return factory.CreateClient(options);
    }

    private static async Task<HttpResponseMessage> Login(
        HttpClient client,
        string email,
        string password)
    {
        var json = JsonConvert.SerializeObject(new LoginInformation(email, password));
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync("api/accounts/login", content);
    }

    [Test]
    public async Task ChallengeCollectionHasFiveMinuteExpiryIndex()
    {
        using var client = CreateClient();
        await client.GetAsync("api/accounts/login-providers");
        var database = factory.Services.GetRequiredService<IMongoDatabase>();
        var collection = database.GetCollection<BsonDocument>(nameof(MenschIdChallenge));

        using var cursor = await collection.Indexes.ListAsync();
        var indexes = await cursor.ToListAsync();

        var expiryIndex = indexes.Single(x => x["name"].AsString == MongoIndexInitializer.ChallengeExpiryIndexName);
        Assert.That(expiryIndex["key"].AsBsonDocument.Names, Is.EqualTo(new[] { nameof(MenschIdChallenge.CreatedTimestamp) }));
        Assert.That(expiryIndex["expireAfterSeconds"].ToDouble(), Is.EqualTo(300));
    }

    [Test]
    public async Task OutdatedExpiryIndexIsReplacedOnStartup()
    {
        using var firstClient = CreateClient();
        await firstClient.GetAsync("api/accounts/login-providers");
        var database = factory.Services.GetRequiredService<IMongoDatabase>();
        var collection = database.GetCollection<BsonDocument>(nameof(MenschIdChallenge));
        await collection.Indexes.DropOneAsync(MongoIndexInitializer.ChallengeExpiryIndexName);
        var outdatedIndex = new CreateIndexModel<BsonDocument>(
            Builders<BsonDocument>.IndexKeys.Ascending(nameof(MenschIdChallenge.CreatedTimestamp)),
            new CreateIndexOptions { Name = "CreatedTimestamp_1", ExpireAfter = TimeSpan.FromDays(1) });
        await collection.Indexes.CreateOneAsync(outdatedIndex);
        var initializer = factory.Services.GetServices<Microsoft.Extensions.Hosting.IHostedService>().OfType<MongoIndexInitializer>().Single();

        await initializer.StartAsync(default);

        using var cursor = await collection.Indexes.ListAsync();
        var indexes = await cursor.ToListAsync();
        var timestampIndexes = indexes.Where(x => x["key"].AsBsonDocument.Names.SequenceEqual(new[] { nameof(MenschIdChallenge.CreatedTimestamp) })).ToList();
        Assert.That(timestampIndexes, Has.Count.EqualTo(1));
        Assert.That(timestampIndexes[0]["expireAfterSeconds"].ToDouble(), Is.EqualTo(300));
    }

    [Test]
    public async Task CredentialEndpointsAreRateLimitedPerClient()
    {
        using var client = CreateClient();

        var statusCodes = new System.Collections.Generic.List<HttpStatusCode>();
        for (var attempt = 0; attempt < 12; attempt++)
        {
            var response = await Login(client, "nobody@example.org", "wrong-password");
            statusCodes.Add(response.StatusCode);
        }

        Assert.That(statusCodes.Take(10), Is.All.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(statusCodes.Skip(10), Is.All.EqualTo(HttpStatusCode.TooManyRequests));
    }

    [Test]
    public async Task OtherEndpointsAreNotAffectedByCredentialRateLimit()
    {
        using var client = CreateClient();
        for (var attempt = 0; attempt < 11; attempt++)
        {
            await Login(client, "nobody@example.org", "wrong-password");
        }

        var response = await client.GetAsync("api/accounts/login-providers");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task AccountIsLockedAfterTooManyWrongPasswords()
    {
        var account = new LocalAccount
        {
            Id = Guid.NewGuid().ToString(),
            Email = "joe@example.org",
            IsEmailVerified = true
        };
        account.PasswordHash = new PasswordHasher<IAccountWithPassword>().HashPassword(account, Password);
        await factory.AccountStore.StoreAsync(account);
        var authenticationModule = factory.Services.CreateScope().ServiceProvider.GetRequiredService<IAuthenticationModule>();

        for (var attempt = 0; attempt < AuthenticationModule.MaximumFailedLoginAttempts; attempt++)
        {
            await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", "wrong-password"));
        }
        var resultWithCorrectPassword = await authenticationModule.AuthenticateLocalByEmailMenschIdOrUsernameAsync(new LoginInformation("joe@example.org", Password));

        Assert.That(resultWithCorrectPassword.IsAuthenticated, Is.False);
        Assert.That(resultWithCorrectPassword.Error, Is.EqualTo(AuthenticationErrorType.AccountLocked));
    }
}
