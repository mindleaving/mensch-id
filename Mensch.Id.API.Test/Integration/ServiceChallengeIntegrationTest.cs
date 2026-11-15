using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Hubs;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Models.ExternalServices;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.Workflow.ExternalServices;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Integration;

public class ServiceChallengeIntegrationTest
{
    private const string Password = "Password123!";
    private const string MenschId = "19900101-ACDEF";
    private static readonly TimeSpan NotificationTimeout = TimeSpan.FromSeconds(10);
    private MenschIdWebApplicationFactory factory;

    private class RegisteredService
    {
        public ExternalService Service { get; init; }
        public string ClientSecret { get; init; }
    }

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

    [Test]
    public async Task AcceptedChallengeIsPushedOnlyToCreatingServiceAndCanBePolled()
    {
        var creatingService = await CreateActiveService();
        var otherService = await CreateActiveService();
        var creatingServiceNotification = new TaskCompletionSource<ServiceChallengeResult>();
        var otherServiceNotification = new TaskCompletionSource<ServiceChallengeResult>();
        await using var creatingServiceConnection = await ConnectServiceHub(creatingService, creatingServiceNotification);
        await using var otherServiceConnection = await ConnectServiceHub(otherService, otherServiceNotification);
        var userClient = await CreateLoggedInUserClient();

        var challenge = await CreateChallenge(creatingService, "service-state");
        var answerResponse = await userClient.PostAsync($"api/service-challenges/{challenge.ChallengeId}/accept", null);

        Assert.That(answerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var completedTask = await Task.WhenAny(creatingServiceNotification.Task, Task.Delay(NotificationTimeout));
        Assert.That(completedTask, Is.SameAs(creatingServiceNotification.Task));
        var pushedResult = await creatingServiceNotification.Task;
        Assert.That(pushedResult.ChallengeId, Is.EqualTo(challenge.ChallengeId));
        await Task.Delay(TimeSpan.FromSeconds(1));
        Assert.That(otherServiceNotification.Task.IsCompleted, Is.False);

        var polledResult = await PollResult(creatingService, challenge.ChallengeId);
        Assert.That(polledResult.Status, Is.EqualTo(ServiceChallengeResultStatus.Answered));
        Assert.That(polledResult.ResultToken, Is.EqualTo(pushedResult.ResultToken));

        var tokenValidation = await ValidateResultToken(pushedResult.ResultToken, creatingService.Service.ClientId);
        Assert.That(tokenValidation.IsValid, Is.True, tokenValidation.Exception?.Message);
        Assert.That(tokenValidation.Claims["sub"], Is.EqualTo(MenschId));
        Assert.That(tokenValidation.Claims["nonce"], Is.EqualTo("service-state"));
        Assert.That(tokenValidation.Claims[ServiceChallengeResultTokenClaims.ChallengeId], Is.EqualTo(challenge.ChallengeId));
        Assert.That(tokenValidation.Claims[ServiceChallengeResultTokenClaims.Result], Is.EqualTo(ServiceChallengeResultTokenClaims.Accepted));
    }

    [Test]
    public async Task ResultTokenIsInvalidForOtherServiceAudience()
    {
        var creatingService = await CreateActiveService();
        var otherService = await CreateActiveService();
        var userClient = await CreateLoggedInUserClient();
        var challenge = await CreateChallenge(creatingService, null);
        await userClient.PostAsync($"api/service-challenges/{challenge.ChallengeId}/accept", null);
        var result = await PollResult(creatingService, challenge.ChallengeId);

        var tokenValidation = await ValidateResultToken(result.ResultToken, otherService.Service.ClientId);

        Assert.That(tokenValidation.IsValid, Is.False);
    }

    [Test]
    public async Task RejectedChallengeHasRejectedResult()
    {
        var service = await CreateActiveService();
        var userClient = await CreateLoggedInUserClient();
        var challenge = await CreateChallenge(service, null);

        var answerResponse = await userClient.PostAsync($"api/service-challenges/{challenge.ChallengeId}/reject", null);

        Assert.That(answerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await PollResult(service, challenge.ChallengeId);
        var tokenValidation = await ValidateResultToken(result.ResultToken, service.Service.ClientId);
        Assert.That(tokenValidation.Claims[ServiceChallengeResultTokenClaims.Result], Is.EqualTo(ServiceChallengeResultTokenClaims.Rejected));
    }

    [Test]
    public async Task ChallengeCanOnlyBeAnsweredOnce()
    {
        var service = await CreateActiveService();
        var userClient = await CreateLoggedInUserClient();
        var challenge = await CreateChallenge(service, null);

        var firstResponse = await userClient.PostAsync($"api/service-challenges/{challenge.ChallengeId}/accept", null);
        var secondResponse = await userClient.PostAsync($"api/service-challenges/{challenge.ChallengeId}/reject", null);

        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task PendingChallengeResultIsPending()
    {
        var service = await CreateActiveService();
        var challenge = await CreateChallenge(service, null);

        var result = await PollResult(service, challenge.ChallengeId);

        Assert.That(result.Status, Is.EqualTo(ServiceChallengeResultStatus.Pending));
        Assert.That(result.ResultToken, Is.Null);
    }

    [Test]
    public async Task OtherServiceCannotPollResult()
    {
        var creatingService = await CreateActiveService();
        var otherService = await CreateActiveService();
        var challenge = await CreateChallenge(creatingService, null);

        var response = await PostResultRequest(otherService, challenge.ChallengeId);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task WrongClientSecretIsRejected()
    {
        var service = await CreateActiveService();
        var serviceWithWrongSecret = new RegisteredService
        {
            Service = service.Service,
            ClientSecret = "wrong-secret"
        };

        var response = await PostChallengeRequest(serviceWithWrongSecret, null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task UserCannotSeeChallengeOfOtherPerson()
    {
        var service = await CreateActiveService();
        var challenge = await CreateChallenge(service, null);
        var otherUserClient = await CreateLoggedInUserClient("other@example.org", "19900101-CDEFG");

        var response = await otherUserClient.PostAsync($"api/service-challenges/{challenge.ChallengeId}/accept", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PendingChallengesCannotBeListedByUser()
    {
        var service = await CreateActiveService();
        await CreateChallenge(service, null);
        var userClient = await CreateLoggedInUserClient("user@example.org", MenschId);

        var response = await userClient.GetAsync("api/service-challenges");

        Assert.That(response.IsSuccessStatusCode, Is.False);
    }

    [Test]
    public async Task UserChallengeHubIsNotAvailable()
    {
        var userClient = await CreateLoggedInUserClient("user@example.org", MenschId);

        var response = await userClient.PostAsync("hubs/user-challenges/negotiate?negotiateVersion=1", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task InactiveServiceCannotGetHubToken()
    {
        var service = await CreateActiveService();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var serviceStore = scope.ServiceProvider.GetRequiredService<IStore<ExternalService, Guid>>();
            service.Service.Status = ExternalServiceStatus.Deactivated;
            await serviceStore.StoreAsync(service.Service);
        }

        var response = await PostClientCredentialsTokenRequest(service);

        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
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

    private async Task<RegisteredService> CreateActiveService()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var serviceStore = scope.ServiceProvider.GetRequiredService<IStore<ExternalService, Guid>>();
        var clientRegistrar = scope.ServiceProvider.GetRequiredService<ExternalServiceClientRegistrar>();
        var service = new ExternalService
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            DisplayName = "Test Service",
            Email = "service@example.org",
            IsEmailVerified = true,
            ContactInformation = "Test contact",
            Status = ExternalServiceStatus.Active,
            CreatedTimestamp = DateTime.UtcNow
        };
        var clientSecret = await clientRegistrar.Register(service);
        await clientRegistrar.Enable(service);
        await serviceStore.StoreAsync(service);
        return new RegisteredService
        {
            Service = service,
            ClientSecret = clientSecret
        };
    }

    private async Task<HubConnection> ConnectServiceHub(
        RegisteredService service,
        TaskCompletionSource<ServiceChallengeResult> notification)
    {
        var tokenResponse = await PostClientCredentialsTokenRequest(service);
        tokenResponse.EnsureSuccessStatusCode();
        var tokenJson = JObject.Parse(await tokenResponse.Content.ReadAsStringAsync());
        var accessToken = tokenJson["access_token"]!.Value<string>();
        var connection = new HubConnectionBuilder()
            .WithUrl(
                $"{MenschIdWebApplicationFactory.ApiBaseUrl}{ServiceChallengeHub.Route}",
                options =>
                {
                    options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Build();
        connection.On<ServiceChallengeResult>(ServiceChallengeHub.ChallengeAnsweredMethodName, x => notification.TrySetResult(x));
        await connection.StartAsync();
        return connection;
    }

    private async Task<HttpResponseMessage> PostClientCredentialsTokenRequest(RegisteredService service)
    {
        var client = CreateClient();
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = service.Service.ClientId.ToString(),
            ["client_secret"] = service.ClientSecret
        };
        var content = new FormUrlEncodedContent(parameters);
        return await client.PostAsync("api/oidc/token", content);
    }

    private async Task<ServiceChallengeCreationResponse> CreateChallenge(
        RegisteredService service,
        string state)
    {
        var response = await PostChallengeRequest(service, state);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ServiceChallengeCreationResponse>(json);
    }

    private async Task<HttpResponseMessage> PostChallengeRequest(
        RegisteredService service,
        string state)
    {
        var client = CreateClient();
        var request = new ServiceChallengeCreationRequest
        {
            ClientId = service.Service.ClientId,
            ClientSecret = service.ClientSecret,
            MenschId = MenschId,
            Description = "Sign in to Test Service\nSecond line",
            State = state
        };
        var content = CreateJsonContent(request);
        return await client.PostAsync("api/service-challenges", content);
    }

    private async Task<ServiceChallengeResult> PollResult(
        RegisteredService service,
        string challengeId)
    {
        var response = await PostResultRequest(service, challengeId);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ServiceChallengeResult>(json);
    }

    private async Task<HttpResponseMessage> PostResultRequest(
        RegisteredService service,
        string challengeId)
    {
        var client = CreateClient();
        var credentials = new ExternalServiceCredentials
        {
            ClientId = service.Service.ClientId,
            ClientSecret = service.ClientSecret
        };
        var content = CreateJsonContent(credentials);
        return await client.PostAsync($"api/service-challenges/{challengeId}/result", content);
    }

    private async Task<TokenValidationResult> ValidateResultToken(
        string resultToken,
        Guid expectedAudience)
    {
        var client = CreateClient();
        var jwksJson = await client.GetStringAsync("api/oidc/.well-known/jwks");
        var signingKeys = new JsonWebKeySet(jwksJson).GetSigningKeys();
        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = $"{MenschIdWebApplicationFactory.ApiBaseUrl}/api/oidc/",
            ValidAudience = expectedAudience.ToString(),
            IssuerSigningKeys = signingKeys
        };
        var tokenHandler = new JsonWebTokenHandler();
        return await tokenHandler.ValidateTokenAsync(resultToken, validationParameters);
    }

    private async Task<HttpClient> CreateLoggedInUserClient(
        string email = "user@example.org",
        string personId = MenschId)
    {
        await CreateVerifiedUser(email, personId);
        var client = CreateClient();
        var response = await PostLogin(client, email);
        response.EnsureSuccessStatusCode();
        return client;
    }

    private async Task CreateVerifiedUser(
        string email,
        string personId)
    {
        var account = new LocalAccount
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            IsEmailVerified = true,
            PersonId = personId
        };
        account.PasswordHash = new PasswordHasher<IAccountWithPassword>().HashPassword(account, Password);
        await factory.AccountStore.StoreAsync(account);
    }

    private static async Task<HttpResponseMessage> PostLogin(
        HttpClient client,
        string email)
    {
        var loginInformation = new LoginInformation(email, Password);
        var content = CreateJsonContent(loginInformation);
        return await client.PostAsync("api/accounts/login", content);
    }

    private static StringContent CreateJsonContent(object body)
    {
        var json = JsonConvert.SerializeObject(body);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
