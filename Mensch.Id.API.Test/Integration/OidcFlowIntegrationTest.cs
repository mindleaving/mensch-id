using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenIddict.Validation;

namespace Mensch.Id.API.Test.Integration;

public class OidcFlowIntegrationTest
{
    private const string Password = "Password123!";
    private const string ClientId = "mensch-id-app";
    private const string RedirectUri = "menschid://auth/callback";
    private const string Scopes = "openid offline_access mensch_id_api";
    private MenschIdWebApplicationFactory factory;

    private class TokenSet
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public string IdToken { get; init; }
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

    private static (string Verifier, string Challenge) CreatePkcePair()
    {
        var verifier = Base64UrlTextEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        var challengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        var challenge = Base64UrlTextEncoder.Encode(challengeBytes);
        return (verifier, challenge);
    }

    private static string BuildAuthorizeUrl(
        string codeChallenge,
        Dictionary<string, string> additionalParameters = null)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["redirect_uri"] = RedirectUri,
            ["response_type"] = "code",
            ["scope"] = Scopes,
            ["state"] = "test-state",
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };
        foreach (var additionalParameter in additionalParameters ?? [])
        {
            parameters[additionalParameter.Key] = additionalParameter.Value;
        }
        return QueryHelpers.AddQueryString("api/oidc/authorize", parameters);
    }

    private static async Task Login(
        HttpClient client,
        string email)
    {
        var loginInformation = new LoginInformation(email, Password);
        var json = JsonConvert.SerializeObject(loginInformation);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("api/accounts/login", content);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<HttpResponseMessage> PostToken(
        HttpClient client,
        Dictionary<string, string> parameters)
    {
        var content = new FormUrlEncodedContent(parameters);
        return await client.PostAsync("api/oidc/token", content);
    }

    private static async Task<TokenSet> ReadTokens(HttpResponseMessage response)
    {
        var json = JObject.Parse(await response.Content.ReadAsStringAsync());
        return new TokenSet
        {
            AccessToken = json["access_token"]?.Value<string>(),
            RefreshToken = json["refresh_token"]?.Value<string>(),
            IdToken = json["id_token"]?.Value<string>()
        };
    }

    private async Task<TokenSet> RunAuthorizationCodeFlow(string email)
    {
        using var browser = CreateClient();
        return await RunAuthorizationCodeFlow(email, browser);
    }

    private async Task<TokenSet> RunAuthorizationCodeFlow(
        string email,
        HttpClient browser)
    {
        var (verifier, challenge) = CreatePkcePair();
        var authorizeUrl = BuildAuthorizeUrl(challenge);

        var loginRedirect = await browser.GetAsync(authorizeUrl);
        Assert.That(loginRedirect.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        var loginLocation = loginRedirect.Headers.Location!;
        var returnUrl = HttpUtility.ParseQueryString(loginLocation.Query)["returnUrl"];
        await Login(browser, email);
        var codeRedirect = await browser.GetAsync(returnUrl);
        Assert.That(codeRedirect.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        var callbackLocation = codeRedirect.Headers.Location!;
        Assert.That(callbackLocation.ToString(), Does.StartWith(RedirectUri));
        var callbackQuery = HttpUtility.ParseQueryString(callbackLocation.Query);
        Assert.That(callbackQuery["state"], Is.EqualTo("test-state"));

        using var app = CreateClient();
        var tokenResponse = await PostToken(app, new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = callbackQuery["code"],
            ["redirect_uri"] = RedirectUri,
            ["client_id"] = ClientId,
            ["code_verifier"] = verifier
        });
        Assert.That(tokenResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), await tokenResponse.Content.ReadAsStringAsync());
        return await ReadTokens(tokenResponse);
    }

    private async Task<HttpResponseMessage> Refresh(string refreshToken)
    {
        using var app = CreateClient();
        return await PostToken(app, new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = ClientId
        });
    }

    private async Task<HttpResponseMessage> GetWithBearer(
        string path,
        string accessToken)
    {
        using var app = CreateClient();
        app.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await app.GetAsync(path);
    }

    private async Task<string> GetPersonIdFromAccessToken(string accessToken)
    {
        var validationService = factory.Services.GetRequiredService<OpenIddictValidationService>();
        var principal = await validationService.ValidateAccessTokenAsync(accessToken);
        return principal.FindFirst(MenschIdClaimTypes.PersonIdClaimName)?.Value;
    }

    [Test]
    public async Task DiscoveryDocumentListsOidcEndpointsUnderApiPath()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("api/oidc/.well-known/openid-configuration");

        var json = JObject.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json["issuer"]!.Value<string>(), Is.EqualTo($"{MenschIdWebApplicationFactory.ApiBaseUrl}/api/oidc/"));
        Assert.That(json["jwks_uri"]!.Value<string>(), Is.EqualTo($"{MenschIdWebApplicationFactory.ApiBaseUrl}/api/oidc/.well-known/jwks"));
        Assert.That(json["authorization_endpoint"]!.Value<string>(), Is.EqualTo($"{MenschIdWebApplicationFactory.ApiBaseUrl}/api/oidc/authorize"));
        Assert.That(json["token_endpoint"]!.Value<string>(), Is.EqualTo($"{MenschIdWebApplicationFactory.ApiBaseUrl}/api/oidc/token"));
        Assert.That(json["end_session_endpoint"]!.Value<string>(), Is.EqualTo($"{MenschIdWebApplicationFactory.ApiBaseUrl}/api/oidc/endsession"));
        Assert.That(json["revocation_endpoint"]!.Value<string>(), Is.EqualTo($"{MenschIdWebApplicationFactory.ApiBaseUrl}/api/oidc/revoke"));
    }

    [Test]
    public async Task RootDiscoveryDocumentIsNotServed()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(".well-known/openid-configuration");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task AuthorizeWithoutSessionRedirectsToWebLogin()
    {
        using var client = CreateClient();
        var (_, challenge) = CreatePkcePair();

        var response = await client.GetAsync(BuildAuthorizeUrl(challenge));

        var location = response.Headers.Location!;
        var returnUrl = HttpUtility.ParseQueryString(location.Query)["returnUrl"];
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(location.ToString(), Does.StartWith($"{MenschIdWebApplicationFactory.WebsiteBaseUrl}/login?returnUrl="));
        Assert.That(returnUrl, Does.StartWith($"{MenschIdWebApplicationFactory.ApiBaseUrl}/api/oidc/authorize?"));
        Assert.That(returnUrl, Does.Contain("code_challenge="));
    }

    [Test]
    public async Task AuthorizeWithProviderRedirectsToUpstreamLogin()
    {
        using var client = CreateClient();
        var (_, challenge) = CreatePkcePair();
        var authorizeUrl = BuildAuthorizeUrl(challenge, new Dictionary<string, string> { ["provider"] = MenschIdWebApplicationFactory.TestProviderId });

        var apiRedirect = await client.GetAsync(authorizeUrl);
        var upstreamRedirect = await client.GetAsync(apiRedirect.Headers.Location!.ToString());

        Assert.That(apiRedirect.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(apiRedirect.Headers.Location!.ToString(), Does.StartWith($"/api/accounts/login/{MenschIdWebApplicationFactory.TestProviderId}?redirectUrl="));
        Assert.That(upstreamRedirect.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(upstreamRedirect.Headers.Location!.ToString(), Does.StartWith(MenschIdWebApplicationFactory.TestProviderAuthorizationEndpoint));
    }

    [Test]
    public async Task AuthorizeWithUnknownProviderIsRejected()
    {
        using var client = CreateClient();
        var (_, challenge) = CreatePkcePair();
        var authorizeUrl = BuildAuthorizeUrl(challenge, new Dictionary<string, string> { ["provider"] = "unknownprovider" });

        var response = await client.GetAsync(authorizeUrl);

        Assert.That(response.Headers.Location?.ToString() ?? string.Empty, Does.Not.Contain("/api/accounts/login/"));
        Assert.That(response.Headers.Location?.ToString() ?? string.Empty, Does.Not.Contain("code="));
    }

    [Test]
    public async Task UnknownClientIsRejected()
    {
        using var client = CreateClient();
        var (_, challenge) = CreatePkcePair();
        var authorizeUrl = BuildAuthorizeUrl(challenge, new Dictionary<string, string> { ["client_id"] = "unknown-client" });

        var response = await client.GetAsync(authorizeUrl);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UnregisteredRedirectUriIsRejected()
    {
        using var client = CreateClient();
        var (_, challenge) = CreatePkcePair();
        var authorizeUrl = BuildAuthorizeUrl(challenge, new Dictionary<string, string> { ["redirect_uri"] = "https://evil.example.com/callback" });

        var response = await client.GetAsync(authorizeUrl);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task MissingPkceIsRejected()
    {
        using var client = CreateClient();
        var authorizeUrl = QueryHelpers.AddQueryString("api/oidc/authorize", new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["redirect_uri"] = RedirectUri,
            ["response_type"] = "code",
            ["scope"] = Scopes
        });

        var response = await client.GetAsync(authorizeUrl);

        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.That(response.StatusCode == HttpStatusCode.BadRequest || location.Contains("error=invalid_request"), Is.True, location);
        Assert.That(location, Does.Not.Contain("code="));
    }

    [Test]
    public async Task WrongCodeVerifierIsRejected()
    {
        await CreateVerifiedUser("joe@example.org");
        using var browser = CreateClient();
        var (_, challenge) = CreatePkcePair();
        var (otherVerifier, _) = CreatePkcePair();
        await Login(browser, "joe@example.org");
        var codeRedirect = await browser.GetAsync(BuildAuthorizeUrl(challenge));
        var code = HttpUtility.ParseQueryString(codeRedirect.Headers.Location!.Query)["code"];

        using var app = CreateClient();
        var tokenResponse = await PostToken(app, new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = RedirectUri,
            ["client_id"] = ClientId,
            ["code_verifier"] = otherVerifier
        });

        Assert.That(tokenResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task FullAppFlowIssuesAccessAndRefreshTokens()
    {
        await CreateVerifiedUser("joe@example.org", "20000101-ACDEF");
        await factory.PersonStore.StoreAsync(new Person("20000101-ACDEF", DateTime.UtcNow, "assigner"));

        var tokens = await RunAuthorizationCodeFlow("joe@example.org");
        var meResponse = await GetWithBearer("api/profiles/me", tokens.AccessToken);
        var refreshResponse = await Refresh(tokens.RefreshToken);
        var refreshedTokens = await ReadTokens(refreshResponse);
        var meAfterRefreshResponse = await GetWithBearer("api/profiles/me", refreshedTokens.AccessToken);

        var meJson = JObject.Parse(await meResponse.Content.ReadAsStringAsync());
        Assert.That(tokens.AccessToken, Is.Not.Null.And.Not.Empty);
        Assert.That(tokens.RefreshToken, Is.Not.Null.And.Not.Empty);
        Assert.That(meResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(meJson["id"]!.Value<string>(), Is.EqualTo("20000101-ACDEF"));
        Assert.That(refreshResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(refreshedTokens.RefreshToken, Is.Not.EqualTo(tokens.RefreshToken));
        Assert.That(meAfterRefreshResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task BearerTokenDoesNotWorkAsSessionCookieAndViceVersa()
    {
        await CreateVerifiedUser("joe@example.org");
        var tokens = await RunAuthorizationCodeFlow("joe@example.org");
        using var client = CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", $"{MenschIdAuthenticationSchemes.SessionCookieName}={tokens.AccessToken}");

        var response = await client.GetAsync("api/accounts/is-logged-in");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task RevokedRefreshTokenCannotBeUsed()
    {
        await CreateVerifiedUser("joe@example.org");
        var tokens = await RunAuthorizationCodeFlow("joe@example.org");
        using var app = CreateClient();
        var revokeContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = tokens.RefreshToken,
            ["token_type_hint"] = "refresh_token",
            ["client_id"] = ClientId
        });

        var revokeResponse = await app.PostAsync("api/oidc/revoke", revokeContent);
        var refreshResponse = await Refresh(tokens.RefreshToken);

        Assert.That(revokeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(refreshResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task RefreshFailsAfterAccountWasDeleted()
    {
        var account = await CreateVerifiedUser("joe@example.org");
        var tokens = await RunAuthorizationCodeFlow("joe@example.org");
        await factory.AccountStore.DeleteAsync(account.Id);

        var refreshResponse = await Refresh(tokens.RefreshToken);

        Assert.That(refreshResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task RefreshAfterTakeOverContainsNewPersonId()
    {
        var account = await CreateVerifiedUser("joe@example.org");
        var tokens = await RunAuthorizationCodeFlow("joe@example.org");
        var personIdBefore = await GetPersonIdFromAccessToken(tokens.AccessToken);
        var assignerControlledProfile = AssignerControlledProfileFactory.Create("20000101-ACDEF", "assigner");
        await factory.AssignerControlledProfileStore.StoreAsync(assignerControlledProfile);
        using var app = CreateClient();
        app.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var body = new TakeControlBody { Id = new MenschId("20000101-ACDEF"), OwnershipSecret = new OwnershipSecret(assignerControlledProfile.OwnershipSecret) };
        var bodyContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var takeControlResponse = await app.PostAsync("api/profiles/take-control", bodyContent);
        var refreshResponse = await Refresh(tokens.RefreshToken);
        var refreshedTokens = await ReadTokens(refreshResponse);
        var personIdAfter = await GetPersonIdFromAccessToken(refreshedTokens.AccessToken);

        Assert.That(takeControlResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(personIdBefore, Is.Null);
        Assert.That(personIdAfter, Is.EqualTo("20000101-ACDEF"));
        Assert.That(account.PersonId, Is.EqualTo("20000101-ACDEF"));
    }

    private static string BuildEndSessionUrl(string idTokenHint)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["post_logout_redirect_uri"] = "menschid://auth/logout"
        };
        if (idTokenHint != null)
            parameters["id_token_hint"] = idTokenHint;
        return QueryHelpers.AddQueryString("api/oidc/endsession", parameters);
    }

    [Test]
    public async Task EndSessionWithMatchingIdTokenHintEndsBrowserSession()
    {
        await CreateVerifiedUser("joe@example.org");
        using var browser = CreateClient();
        var tokens = await RunAuthorizationCodeFlow("joe@example.org", browser);

        var endSessionResponse = await browser.GetAsync(BuildEndSessionUrl(tokens.IdToken));
        var isLoggedInResponse = await browser.GetAsync("api/accounts/is-logged-in");

        Assert.That(tokens.IdToken, Is.Not.Null.And.Not.Empty);
        Assert.That(endSessionResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(endSessionResponse.Headers.Location!.ToString(), Does.StartWith("menschid://auth/logout"));
        Assert.That(isLoggedInResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task EndSessionWithoutIdTokenHintDoesNotEndBrowserSession()
    {
        await CreateVerifiedUser("joe@example.org");
        using var browser = CreateClient();
        await Login(browser, "joe@example.org");

        await browser.GetAsync(BuildEndSessionUrl(null));
        var isLoggedInResponse = await browser.GetAsync("api/accounts/is-logged-in");

        Assert.That(isLoggedInResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task EndSessionWithIdTokenOfOtherAccountDoesNotEndBrowserSession()
    {
        await CreateVerifiedUser("joe@example.org");
        await CreateVerifiedUser("eve@example.org");
        var attackerTokens = await RunAuthorizationCodeFlow("eve@example.org");
        using var victimBrowser = CreateClient();
        await Login(victimBrowser, "joe@example.org");

        await victimBrowser.GetAsync(BuildEndSessionUrl(attackerTokens.IdToken));
        var isLoggedInResponse = await victimBrowser.GetAsync("api/accounts/is-logged-in");

        Assert.That(isLoggedInResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task EndSessionWithForgedIdTokenHintDoesNotEndBrowserSession()
    {
        await CreateVerifiedUser("joe@example.org");
        using var browser = CreateClient();
        await Login(browser, "joe@example.org");
        const string ForgedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ4In0.c2lnbmF0dXJl";

        await browser.GetAsync(BuildEndSessionUrl(ForgedToken));
        var isLoggedInResponse = await browser.GetAsync("api/accounts/is-logged-in");

        Assert.That(isLoggedInResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task AccessTokenCannotReadChallengeOfOtherPerson()
    {
        await CreateVerifiedUser("joe@example.org", "20000101-ACDEF");
        var foreignChallenge = new MenschIdChallenge
        {
            Id = Guid.NewGuid().ToString(),
            MenschId = "19990101-ACDEF",
            ChallengeShortId = "ABCDEF",
            ChallengeSecret = "SECRET",
            CreatedTimestamp = DateTime.UtcNow
        };
        await factory.ChallengeStore.StoreAsync(foreignChallenge);
        var tokens = await RunAuthorizationCodeFlow("joe@example.org");

        var response = await GetWithBearer($"api/challenges/{foreignChallenge.Id}", tokens.AccessToken);

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(content, Does.Not.Contain("SECRET"));
    }

    [Test]
    public async Task OwnChallengeCanBeReadByIdAndSearchedByShortId()
    {
        await CreateVerifiedUser("joe@example.org", "20000101-ACDEF");
        var ownChallenge = new MenschIdChallenge
        {
            Id = Guid.NewGuid().ToString(),
            MenschId = "20000101-ACDEF",
            ChallengeShortId = "QWERTY",
            ChallengeSecret = "MYSECRET",
            CreatedTimestamp = DateTime.UtcNow
        };
        await factory.ChallengeStore.StoreAsync(ownChallenge);
        var tokens = await RunAuthorizationCodeFlow("joe@example.org");

        var byIdResponse = await GetWithBearer($"api/challenges/{ownChallenge.Id}", tokens.AccessToken);
        var searchResponse = await GetWithBearer("api/challenges?searchText=QW&count=10", tokens.AccessToken);

        var byIdJson = JObject.Parse(await byIdResponse.Content.ReadAsStringAsync());
        var searchJson = JArray.Parse(await searchResponse.Content.ReadAsStringAsync());
        Assert.That(byIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(byIdJson["challengeSecret"]!.Value<string>(), Is.EqualTo("MYSECRET"));
        Assert.That(searchJson, Has.Count.EqualTo(1));
        Assert.That(searchJson[0]!["challengeShortId"]!.Value<string>(), Is.EqualTo("QWERTY"));
    }
}
