using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Models.ExternalServices;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenIddict.Validation;

namespace Mensch.Id.API.Test.Integration;

public class AdminIntegrationTest
{
    private const string AdminUsername = "admin";
    private const string AssignerUsername = "clinic";
    private const string UserEmail = "user@example.org";
    private MenschIdWebApplicationFactory factory;
    private OidcFlowRunner flowRunner;
    private AssignerAccount assignerAccount;

    [SetUp]
    public async Task Setup()
    {
        factory = new MenschIdWebApplicationFactory();
        flowRunner = new OidcFlowRunner(factory);
        await StoreProfessionalAccount(new AdminAccount { Id = Guid.NewGuid().ToString(), Username = AdminUsername });
        assignerAccount = new AssignerAccount
        {
            Id = Guid.NewGuid().ToString(),
            Username = AssignerUsername,
            Name = "Clinic",
            Contact = new Contact { Name = "Clinic", Email = "clinic@example.org" }
        };
        await StoreProfessionalAccount(assignerAccount);
        await flowRunner.CreateVerifiedUser(UserEmail, "19900101-ACDEF");
    }

    [TearDown]
    public void TearDown()
    {
        factory.Dispose();
    }

    [Test]
    public async Task ExternalServiceLifecycle()
    {
        var registration = await RegisterService("Lifecycle Service");
        Assert.That((await RequestServiceToken(registration)).StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
        Assert.That((await CreateChallenge(registration)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using var admin = await LoginProfessional(AdminUsername);

        var activationBeforeVerification = await admin.PostAsync($"api/externalServices/{registration.Id}/activate", null);
        Assert.That(activationBeforeVerification.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        await VerifyServiceEmail(registration.Id);
        var activation = await admin.PostAsync($"api/externalServices/{registration.Id}/activate", null);
        Assert.That(activation.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var tokenResponse = await RequestServiceToken(registration);
        Assert.That(tokenResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await tokenResponse.Content.ReadAsStringAsync());
        var accessToken = JObject.Parse(await tokenResponse.Content.ReadAsStringAsync())["access_token"]!.Value<string>();
        Assert.That((await CreateChallenge(registration)).StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var deactivation = await admin.PostAsync($"api/externalServices/{registration.Id}/deactivate", null);
        Assert.That(deactivation.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await RequestServiceToken(registration)).StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
        Assert.That((await CreateChallenge(registration)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var validationService = factory.Services.GetRequiredService<OpenIddictValidationService>();
        Assert.That(async () => await validationService.ValidateAccessTokenAsync(accessToken), Throws.Exception);
    }

    [Test]
    public async Task WrongVerificationTokenIsRejected()
    {
        var registration = await RegisterService("Wrong Token Service");
        using var client = flowRunner.CreateClient();

        var response = await client.GetAsync($"api/externalServices/{registration.Id}/verify-email?token=invalid");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task RegistrationWithoutEmailIsRejected()
    {
        using var client = flowRunner.CreateClient();
        var body = new { displayName = "No Mail", contactInformation = "Example Ltd." };

        var response = await client.PostAsync("api/externalServices", CreateJsonContent(body));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task SecretRotationInvalidatesOldSecret()
    {
        var registration = await RegisterService("Rotating Service");
        await VerifyServiceEmail(registration.Id);
        using var admin = await LoginProfessional(AdminUsername);
        await admin.PostAsync($"api/externalServices/{registration.Id}/activate", null);
        using var client = flowRunner.CreateClient();

        var rotationResponse = await client.PostAsync("api/externalServices/rotate-secret", CreateJsonContent(registration));

        Assert.That(rotationResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var newSecret = JObject.Parse(await rotationResponse.Content.ReadAsStringAsync())["client_secret"]!.Value<string>();
        Assert.That((await RequestServiceToken(registration)).StatusCode, Is.Not.EqualTo(HttpStatusCode.OK));
        var rotatedRegistration = new ExternalServiceRegistrationResponse
        {
            Id = registration.Id,
            ClientId = registration.ClientId,
            ClientSecret = newSecret
        };
        Assert.That((await RequestServiceToken(rotatedRegistration)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ExternalServicesCanBeFilteredBySearchTextAndStatus()
    {
        var activeRegistration = await RegisterService("Alpha Shop");
        await RegisterService("Beta Portal");
        await VerifyServiceEmail(activeRegistration.Id);
        using var admin = await LoginProfessional(AdminUsername);
        await admin.PostAsync($"api/externalServices/{activeRegistration.Id}/activate", null);

        var bySearchText = await GetServices(admin, "searchText=alpha");
        var byClientId = await GetServices(admin, $"searchText={activeRegistration.ClientId}");
        var pending = await GetServices(admin, "status=Pending");
        var all = await GetServices(admin, string.Empty);

        Assert.That(bySearchText.Select(x => x.DisplayName), Is.EqualTo(new[] { "Alpha Shop" }));
        Assert.That(byClientId.Select(x => x.Id), Is.EqualTo(new[] { activeRegistration.Id }));
        Assert.That(pending.Select(x => x.DisplayName), Is.EqualTo(new[] { "Beta Portal" }));
        Assert.That(all, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task DeactivatedAssignerIsLoggedOutAndCannotLogIn()
    {
        using var assigner = await LoginProfessional(AssignerUsername);
        Assert.That((await assigner.GetAsync("api/assigner/assigned-ids")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var admin = await LoginProfessional(AdminUsername);

        var deactivation = await admin.PostAsync($"api/admin/assigners/{assignerAccount.Id}/deactivate", null);

        Assert.That(deactivation.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await assigner.GetAsync("api/assigner/assigned-ids")).IsSuccessStatusCode, Is.False);
        Assert.That((await TryLoginProfessional(AssignerUsername)).IsSuccessStatusCode, Is.False);
        var activation = await admin.PostAsync($"api/admin/assigners/{assignerAccount.Id}/activate", null);
        Assert.That(activation.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await TryLoginProfessional(AssignerUsername)).IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task AssignersCanBeFilteredBySearchTextAndStatus()
    {
        using var admin = await LoginProfessional(AdminUsername);
        await admin.PostAsync($"api/admin/assigners/{assignerAccount.Id}/deactivate", null);

        var deactivated = await GetAssigners(admin, "status=Deactivated");
        var activated = await GetAssigners(admin, "status=Activated");
        var bySearchText = await GetAssigners(admin, "searchText=CLINIC@example");
        var noMatch = await GetAssigners(admin, "searchText=hospital");

        Assert.That(deactivated.Single().Id, Is.EqualTo(assignerAccount.Id));
        Assert.That(deactivated.Single().IsActivated, Is.False);
        Assert.That(activated, Is.Empty);
        Assert.That(bySearchText, Has.Count.EqualTo(1));
        Assert.That(noMatch, Is.Empty);
    }

    [Test]
    public async Task RegularUsersCannotUseAdminEndpoints()
    {
        using var user = await flowRunner.CreateLoggedInBrowser(UserEmail);

        Assert.That((await user.GetAsync("api/externalServices")).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That((await user.GetAsync("api/admin/assigners")).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That((await user.PostAsync($"api/admin/assigners/{assignerAccount.Id}/deactivate", null)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private async Task StoreProfessionalAccount(ProfessionalAccount account)
    {
        account.PasswordHash = new PasswordHasher<IAccountWithPassword>().HashPassword(account, OidcFlowRunner.Password);
        await factory.AccountStore.StoreAsync(account);
    }

    private async Task<HttpClient> LoginProfessional(string username)
    {
        var client = flowRunner.CreateClient();
        var response = await PostLogin(client, username);
        response.EnsureSuccessStatusCode();
        return client;
    }

    private async Task<HttpResponseMessage> TryLoginProfessional(string username)
    {
        using var client = flowRunner.CreateClient();
        return await PostLogin(client, username);
    }

    private static async Task<HttpResponseMessage> PostLogin(
        HttpClient client,
        string username)
    {
        var loginInformation = new LoginInformation(username, OidcFlowRunner.Password);
        return await client.PostAsync("api/accounts/login", CreateJsonContent(loginInformation));
    }

    private async Task<ExternalServiceRegistrationResponse> RegisterService(string displayName)
    {
        using var client = flowRunner.CreateClient();
        var body = new ExternalServiceRegistration
        {
            DisplayName = displayName,
            Email = "service@example.org",
            ContactInformation = "Example Ltd.\nExample Street 1"
        };
        var response = await client.PostAsync("api/externalServices", CreateJsonContent(body));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ExternalServiceRegistrationResponse>(json);
    }

    private async Task VerifyServiceEmail(Guid serviceId)
    {
        var email = factory.EmailSender.ExternalServiceVerificationEmails.Last(x => x.ServiceId == serviceId);
        using var client = flowRunner.CreateClient();
        var token = Uri.EscapeDataString(email.VerificationToken);
        var response = await client.GetAsync($"api/externalServices/{serviceId}/verify-email?token={token}");
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> RequestServiceToken(ExternalServiceCredentials credentials)
    {
        using var client = flowRunner.CreateClient();
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = credentials.ClientId.ToString(),
            ["client_secret"] = credentials.ClientSecret
        };
        return await client.PostAsync("api/oidc/token", new FormUrlEncodedContent(parameters));
    }

    private async Task<HttpResponseMessage> CreateChallenge(ExternalServiceCredentials credentials)
    {
        using var client = flowRunner.CreateClient();
        var request = new ServiceChallengeCreationRequest
        {
            ClientId = credentials.ClientId,
            ClientSecret = credentials.ClientSecret,
            MenschId = "19900101-ACDEF",
            Description = "Sign in"
        };
        return await client.PostAsync("api/service-challenges", CreateJsonContent(request));
    }

    private static async Task<List<ExternalService>> GetServices(
        HttpClient admin,
        string query)
    {
        var response = await admin.GetAsync($"api/externalServices?{query}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<ExternalService>>(json);
    }

    private static async Task<List<AdminAssignerViewModel>> GetAssigners(
        HttpClient admin,
        string query)
    {
        var response = await admin.GetAsync($"api/admin/assigners?{query}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<AdminAssignerViewModel>>(json);
    }

    private static StringContent CreateJsonContent(object body)
    {
        var json = JsonConvert.SerializeObject(body);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
