using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Test.TestInfrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Integration;

public class ApiIntegrationTest
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
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        };
        return factory.CreateClient(options);
    }

    private static StringContent ToJsonContent(object body)
    {
        var json = JsonConvert.SerializeObject(body);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task CreateAssigner(string username)
    {
        var account = new AssignerAccount
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Clinic",
            Username = username
        };
        account.PasswordHash = new PasswordHasher<IAccountWithPassword>().HashPassword(account, Password);
        await factory.AccountStore.StoreAsync(account);
    }

    private async Task<LocalAccount> CreateVerifiedUser(
        string email,
        string personId = null)
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
        return account;
    }

    private static async Task<HttpResponseMessage> Login(
        HttpClient client,
        string emailMenschIdOrUsername)
    {
        var loginInformation = new LoginInformation(emailMenschIdOrUsername, Password);
        var content = ToJsonContent(loginInformation);
        return await client.PostAsync("api/accounts/login", content);
    }

    private async Task<string> CreateIdAsAssigner(HttpClient assignerClient)
    {
        var candidateResponse = await assignerClient.GetAsync("api/assigner/candidate?birthDate=2000-01-01");
        candidateResponse.EnsureSuccessStatusCode();
        var candidateJson = JObject.Parse(await candidateResponse.Content.ReadAsStringAsync());
        var idCandidate = candidateJson["idCandidates"]![0]!.Value<string>();
        var claimResponse = await assignerClient.PostAsync($"api/assigner/claim/{idCandidate}", null);
        claimResponse.EnsureSuccessStatusCode();
        return idCandidate;
    }

    [Test]
    public async Task LoginProvidersAreListedWithoutSecrets()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("api/accounts/login-providers");

        var json = await response.Content.ReadAsStringAsync();
        var providers = JArray.Parse(json);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(providers.Select(x => x["id"]!.Value<string>()), Is.EqualTo(new[]
        {
            MenschIdWebApplicationFactory.TestProviderId,
            MenschIdWebApplicationFactory.SecondProviderId
        }));
        Assert.That(providers[0]!["displayName"]!.Value<string>(), Is.EqualTo(MenschIdWebApplicationFactory.TestProviderDisplayName));
        Assert.That(json, Does.Not.Contain(MenschIdWebApplicationFactory.TestProviderClientSecret));
        Assert.That(json, Does.Not.Contain(MenschIdWebApplicationFactory.TestProviderClientId));
        Assert.That(json, Does.Not.Contain("SecondClientSecretValue"));
        Assert.That(json, Does.Not.Contain("example.org"));
    }

    [Test]
    public async Task UnknownLoginProviderReturnsBadRequest()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("api/accounts/login/unknownprovider");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task MalformedLoginProviderReturnsBadRequest()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("api/accounts/login/not-valid");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ForeignRedirectUrlIsRejected()
    {
        using var client = CreateClient();
        var redirectUrl = HttpUtility.UrlEncode("https://evil.example.com/steal");

        var response = await client.GetAsync($"api/accounts/login/{MenschIdWebApplicationFactory.TestProviderId}?redirectUrl={redirectUrl}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ConfiguredLoginProviderRedirectsToAuthorityWithPkceAndOpenIdScopeOnly()
    {
        using var client = CreateClient();
        var redirectUrl = HttpUtility.UrlEncode($"{MenschIdWebApplicationFactory.WebsiteBaseUrl}/login/redirect");

        var response = await client.GetAsync($"api/accounts/login/{MenschIdWebApplicationFactory.TestProviderId}?redirectUrl={redirectUrl}");

        var location = response.Headers.Location!;
        var query = HttpUtility.ParseQueryString(location.Query);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(location.GetLeftPart(UriPartial.Path), Is.EqualTo(MenschIdWebApplicationFactory.TestProviderAuthorizationEndpoint));
        Assert.That(query["client_id"], Is.EqualTo(MenschIdWebApplicationFactory.TestProviderClientId));
        Assert.That(query["response_type"], Is.EqualTo("code"));
        Assert.That(query["scope"], Is.EqualTo("openid"));
        Assert.That(query["code_challenge"], Is.Not.Null.And.Not.Empty);
        Assert.That(query["code_challenge_method"], Is.EqualTo("S256"));
        Assert.That(query["redirect_uri"], Does.EndWith($"/api/accounts/external-login/{MenschIdWebApplicationFactory.TestProviderId}"));
        Assert.That(location.ToString(), Does.Not.Contain(MenschIdWebApplicationFactory.TestProviderClientSecret));
    }

    [Test]
    public async Task LocalLoginSetsHttpOnlySessionCookie()
    {
        await CreateVerifiedUser("joe@example.org");
        using var client = CreateClient();

        var response = await Login(client, "joe@example.org");

        var setCookieHeaders = response.Headers.GetValues("Set-Cookie").ToList();
        var sessionCookie = setCookieHeaders.Single(x => x.StartsWith(MenschIdAuthenticationSchemes.SessionCookieName));
        var isLoggedInResponse = await client.GetAsync("api/accounts/is-logged-in");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(sessionCookie, Does.Contain("httponly").IgnoreCase);
        Assert.That(sessionCookie, Does.Contain("secure").IgnoreCase);
        Assert.That(sessionCookie, Does.Contain("samesite=lax").IgnoreCase);
        Assert.That(isLoggedInResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task WrongPasswordIsUnauthorized()
    {
        await CreateVerifiedUser("joe@example.org");
        using var client = CreateClient();
        var content = ToJsonContent(new LoginInformation("joe@example.org", "wrong"));

        var response = await client.PostAsync("api/accounts/login", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(response.Headers.Contains("Set-Cookie"), Is.False);
    }

    [Test]
    public async Task ProtectedEndpointsRequireAuthentication()
    {
        using var client = CreateClient();

        var meResponse = await client.GetAsync("api/profiles/me");
        var accountsResponse = await client.GetAsync("api/accounts");
        var isLoggedInResponse = await client.GetAsync("api/accounts/is-logged-in");

        Assert.That(meResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(accountsResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(isLoggedInResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ForgedSessionCookieAndBearerTokenAreRejected()
    {
        using var cookieClient = CreateClient();
        using var bearerClient = CreateClient();
        const string ForgedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2NvdW50VHlwZSI6IkFkbWluIn0.c2lnbmF0dXJl";
        cookieClient.DefaultRequestHeaders.Add("Cookie", $"{MenschIdAuthenticationSchemes.SessionCookieName}={ForgedToken}");
        bearerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ForgedToken);

        var cookieResponse = await cookieClient.GetAsync("api/accounts/is-logged-in");
        var bearerResponse = await bearerClient.GetAsync("api/accounts/is-logged-in");

        Assert.That(cookieResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(bearerResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task LogoutEndsSession()
    {
        await CreateVerifiedUser("joe@example.org");
        using var client = CreateClient();
        await Login(client, "joe@example.org");

        var logoutResponse = await client.PostAsync("api/accounts/logout", null);
        var isLoggedInResponse = await client.GetAsync("api/accounts/is-logged-in");

        Assert.That(logoutResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(isLoggedInResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task NormalUsersCannotCreateMenschIds()
    {
        await CreateVerifiedUser("joe@example.org");
        using var client = CreateClient();
        await Login(client, "joe@example.org");

        var newProfileResponse = await client.GetAsync("api/profiles/new?birthDate=2000-01-01");
        var claimResponse = await client.PostAsync("api/profiles/claim/20000101-ACDEF", null);
        var assignerCandidateResponse = await client.GetAsync("api/assigner/candidate?birthDate=2000-01-01");
        var assignerClaimResponse = await client.PostAsync("api/assigner/claim/20000101-ACDEF", null);

        Assert.That(newProfileResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound).Or.EqualTo(HttpStatusCode.MethodNotAllowed));
        Assert.That(claimResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound).Or.EqualTo(HttpStatusCode.MethodNotAllowed));
        Assert.That(assignerCandidateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(assignerClaimResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(factory.PersonStore.Items, Is.Empty);
    }

    [Test]
    [TestCase("not-a-mensch-id")]
    [TestCase("20000101-ACDEB")]
    [TestCase("x20000101-ACDEF")]
    public async Task MalformedMenschIdInRouteIsRejected(string menschId)
    {
        await CreateAssigner("clinic");
        using var assignerClient = CreateClient();
        await Login(assignerClient, "clinic");

        var challengeResponse = await assignerClient.PostAsync($"api/id/{menschId}/challenge", null);
        var claimResponse = await assignerClient.PostAsync($"api/assigner/claim/{menschId}", null);
        var profileResponse = await assignerClient.GetAsync($"api/assigner/profiles/{menschId}");

        Assert.That(challengeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(claimResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(profileResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(factory.PersonStore.Items, Is.Empty);
    }

    [Test]
    public async Task LowerCaseMenschIdInRouteIsNormalized()
    {
        await CreateAssigner("clinic");
        using var assignerClient = CreateClient();
        await Login(assignerClient, "clinic");
        var menschId = await CreateIdAsAssigner(assignerClient);

        var response = await assignerClient.GetAsync($"api/assigner/profiles/{menschId.ToLowerInvariant()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    [TestCase("{\"id\":\"not-a-mensch-id\",\"ownershipSecret\":\"ACDEFGHJKLMNPQRSTUVWXYZ2345679\"}")]
    [TestCase("{\"id\":\"20000101-ACDEF\",\"ownershipSecret\":\"too-short\"}")]
    [TestCase("{\"id\":\"20000101-ACDEF\",\"ownershipSecret\":\"acdefghjklmnpqrstuvwxyz2345679\"}")]
    [TestCase("{\"ownershipSecret\":\"ACDEFGHJKLMNPQRSTUVWXYZ2345679\"}")]
    [TestCase("{\"id\":\"20000101-ACDEF\"}")]
    public async Task MalformedTakeControlBodyIsRejected(string json)
    {
        await CreateVerifiedUser("joe@example.org");
        using var client = CreateClient();
        await Login(client, "joe@example.org");
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("api/profiles/take-control", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AnonymousUsersCannotCreateMenschIds()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("api/assigner/claim/20000101-ACDEF", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized).Or.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(factory.PersonStore.Items, Is.Empty);
    }

    [Test]
    public async Task AssignersCannotTakeControlOfIds()
    {
        await CreateAssigner("clinic");
        using var assignerClient = CreateClient();
        await Login(assignerClient, "clinic");
        var menschId = await CreateIdAsAssigner(assignerClient);
        var ownershipSecret = factory.AssignerControlledProfileStore.Items.Single().OwnershipSecret;
        var body = new TakeControlBody { Id = new MenschId(menschId), OwnershipSecret = new OwnershipSecret(ownershipSecret) };

        var response = await assignerClient.PostAsync("api/profiles/take-control", ToJsonContent(body));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(await factory.AssignerControlledProfileStore.ExistsAsync(menschId), Is.True);
    }

    [Test]
    public async Task AssignerCreatesIdAndUserTakesControlWithOwnershipSecret()
    {
        await CreateAssigner("clinic");
        var user = await CreateVerifiedUser("joe@example.org");
        using var assignerClient = CreateClient();
        using var userClient = CreateClient();
        await Login(assignerClient, "clinic");
        await Login(userClient, "joe@example.org");

        var menschId = await CreateIdAsAssigner(assignerClient);
        var profileResponse = await assignerClient.GetAsync($"api/assigner/profiles/{menschId}");
        var profileJson = JObject.Parse(await profileResponse.Content.ReadAsStringAsync());
        var ownershipSecret = profileJson["ownershipSecret"]!.Value<string>();
        var wrongBody = new TakeControlBody { Id = new MenschId(menschId), OwnershipSecret = new OwnershipSecret(new string('A', ownershipSecret!.Length)) };
        var wrongSecretResponse = await userClient.PostAsync("api/profiles/take-control", ToJsonContent(wrongBody));
        var body = new TakeControlBody { Id = new MenschId(menschId), OwnershipSecret = new OwnershipSecret(ownershipSecret) };
        var takeControlResponse = await userClient.PostAsync("api/profiles/take-control", ToJsonContent(body));
        var meResponse = await userClient.GetAsync("api/profiles/me");

        var meJson = JObject.Parse(await meResponse.Content.ReadAsStringAsync());
        var storedUser = (LocalAccount)await factory.AccountStore.GetByIdAsync(user.Id);
        Assert.That(wrongSecretResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(takeControlResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(storedUser.PersonId, Is.EqualTo(menschId));
        Assert.That(await factory.AssignerControlledProfileStore.ExistsAsync(menschId), Is.False);
        Assert.That(meResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(meJson["id"]!.Value<string>(), Is.EqualTo(menschId));
        Assert.That(meJson["loginProviders"]!.Values<string>(), Is.EqualTo(new[] { "local" }));
    }

    [Test]
    public async Task OwnershipSecretCannotBeReusedBySecondUser()
    {
        await CreateAssigner("clinic");
        await CreateVerifiedUser("joe@example.org");
        await CreateVerifiedUser("jane@example.org");
        using var assignerClient = CreateClient();
        using var firstUserClient = CreateClient();
        using var secondUserClient = CreateClient();
        await Login(assignerClient, "clinic");
        await Login(firstUserClient, "joe@example.org");
        await Login(secondUserClient, "jane@example.org");
        var menschId = await CreateIdAsAssigner(assignerClient);
        var ownershipSecret = factory.AssignerControlledProfileStore.Items.Single().OwnershipSecret;
        var body = new TakeControlBody { Id = new MenschId(menschId), OwnershipSecret = new OwnershipSecret(ownershipSecret) };

        var firstResponse = await firstUserClient.PostAsync("api/profiles/take-control", ToJsonContent(body));
        var secondResponse = await secondUserClient.PostAsync("api/profiles/take-control", ToJsonContent(body));

        Assert.That(firstResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task MyAccountsDoNotExposePasswordHashesOrTokenNonces()
    {
        var user = await CreateVerifiedUser("joe@example.org", "20000101-ACDEF");
        user.PasswordResetNonce = "SomePasswordResetNonceValue";
        user.EmailVerificationNonce = "SomeEmailVerificationNonceValue";
        await factory.AccountStore.StoreAsync(user);
        using var client = CreateClient();
        await Login(client, "joe@example.org");

        var response = await client.GetAsync("api/accounts");

        var json = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json, Does.Contain("joe@example.org"));
        Assert.That(json, Does.Not.Contain(user.PasswordHash));
        Assert.That(json, Does.Not.Contain("passwordHash").IgnoreCase);
        Assert.That(json, Does.Not.Contain("SomePasswordResetNonceValue"));
        Assert.That(json, Does.Not.Contain("SomeEmailVerificationNonceValue"));
    }

    [Test]
    public async Task RegistrationWithEmailVerificationAllowsLogin()
    {
        using var client = CreateClient();
        var registration = new RegistrationInformation
        {
            AccountType = AccountType.Local,
            Email = "new@example.org",
            Password = Password
        };

        var registrationResponse = await client.PostAsync("api/accounts/register-local", ToJsonContent(registration));
        var loginBeforeVerification = await Login(client, "new@example.org");
        var verificationEmail = factory.EmailSender.VerificationEmails.Single();
        var token = HttpUtility.UrlEncode(verificationEmail.VerificationToken);
        var verifyResponse = await client.GetAsync($"api/accounts/{verificationEmail.AccountId}/verify-email?token={token}");
        var loginAfterVerification = await Login(client, "new@example.org");

        var account = (LocalAccount)await factory.AccountStore.GetByIdAsync(verificationEmail.AccountId);
        Assert.That(registrationResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(loginBeforeVerification.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(verifyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(loginAfterVerification.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(account.PersonId, Is.Null);
    }
}
