using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mensch.Id.API.Test.TestInfrastructure;

public class OidcTokenSet
{
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
}

public class OidcFlowRunner
{
    public const string Password = "Password123!";
    public const string BrowserUserAgent = "TestBrowser/1.0";
    public const string AppUserAgent = "TestApp/1.0";
    private readonly MenschIdWebApplicationFactory factory;

    public OidcFlowRunner(MenschIdWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    public HttpClient CreateClient(string userAgent = BrowserUserAgent)
    {
        var options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri(MenschIdWebApplicationFactory.ApiBaseUrl),
            AllowAutoRedirect = false,
            HandleCookies = true
        };
        var client = factory.CreateClient(options);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        return client;
    }

    public async Task CreateVerifiedUser(
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

    public async Task<HttpClient> CreateLoggedInBrowser(string email)
    {
        var browser = CreateClient();
        await Login(browser, email);
        return browser;
    }

    public async Task Login(
        HttpClient client,
        string email)
    {
        var loginInformation = new LoginInformation(email, Password);
        var json = JsonConvert.SerializeObject(loginInformation);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("api/accounts/login", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<OidcTokenSet> RunAuthorizationCodeFlow(string email)
    {
        using var browser = CreateClient(AppUserAgent);
        var verifierBytes = RandomNumberGenerator.GetBytes(32);
        var verifier = Base64UrlTextEncoder.Encode(verifierBytes);
        var challengeBytes = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        var challenge = Base64UrlTextEncoder.Encode(challengeBytes);
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = MenschIdWebApplicationFactory.AppClientId,
            ["redirect_uri"] = MenschIdWebApplicationFactory.AppRedirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid offline_access mensch_id_api",
            ["state"] = "test-state",
            ["code_challenge"] = challenge,
            ["code_challenge_method"] = "S256"
        };
        var authorizeUrl = QueryHelpers.AddQueryString("api/oidc/authorize", parameters);
        var loginRedirect = await browser.GetAsync(authorizeUrl);
        var returnUrl = HttpUtility.ParseQueryString(loginRedirect.Headers.Location!.Query)["returnUrl"];
        await Login(browser, email);
        var codeRedirect = await browser.GetAsync(returnUrl);
        var code = HttpUtility.ParseQueryString(codeRedirect.Headers.Location!.Query)["code"];
        var tokenResponse = await PostToken(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = MenschIdWebApplicationFactory.AppRedirectUri,
            ["client_id"] = MenschIdWebApplicationFactory.AppClientId,
            ["code_verifier"] = verifier
        });
        tokenResponse.EnsureSuccessStatusCode();
        var json = JObject.Parse(await tokenResponse.Content.ReadAsStringAsync());
        return new OidcTokenSet
        {
            AccessToken = json["access_token"]?.Value<string>(),
            RefreshToken = json["refresh_token"]?.Value<string>()
        };
    }

    public async Task<HttpResponseMessage> Refresh(string refreshToken)
    {
        return await PostToken(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = MenschIdWebApplicationFactory.AppClientId
        });
    }

    public HttpClient CreateBearerClient(string accessToken)
    {
        var client = CreateClient(AppUserAgent);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private async Task<HttpResponseMessage> PostToken(Dictionary<string, string> parameters)
    {
        using var client = CreateClient(AppUserAgent);
        var content = new FormUrlEncodedContent(parameters);
        return await client.PostAsync("api/oidc/token", content);
    }
}
