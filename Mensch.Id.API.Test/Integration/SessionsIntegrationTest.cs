using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.ViewModels;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Integration;

public class SessionsIntegrationTest
{
    private const string Email = "user@example.org";
    private const string SecondEmail = "second@example.org";
    private const string OtherPersonEmail = "other@example.org";
    private const string MenschId = "19900101-ACDEF";
    private const string OtherMenschId = "19900101-CDEFG";
    private MenschIdWebApplicationFactory factory;
    private OidcFlowRunner flowRunner;

    [SetUp]
    public async Task Setup()
    {
        factory = new MenschIdWebApplicationFactory();
        flowRunner = new OidcFlowRunner(factory);
        await flowRunner.CreateVerifiedUser(Email, MenschId);
        await flowRunner.CreateVerifiedUser(SecondEmail, MenschId);
        await flowRunner.CreateVerifiedUser(OtherPersonEmail, OtherMenschId);
    }

    [TearDown]
    public void TearDown()
    {
        factory.Dispose();
    }

    [Test]
    public async Task AppAndWebSessionsAreListed()
    {
        await flowRunner.RunAuthorizationCodeFlow(Email);
        using var browser = await flowRunner.CreateLoggedInBrowser(Email);

        var sessions = await GetSessions(browser);

        var appSessions = sessions.Where(x => x.Type == SessionType.App).ToList();
        var webSessions = sessions.Where(x => x.Type == SessionType.Web).ToList();
        Assert.That(appSessions, Has.Count.EqualTo(1));
        Assert.That(appSessions[0].ClientName, Is.EqualTo("mensch.ID App"));
        Assert.That(appSessions[0].UserAgent, Is.EqualTo(OidcFlowRunner.AppUserAgent));
        Assert.That(webSessions.Count(x => x.IsCurrent), Is.EqualTo(1));
        Assert.That(webSessions.Single(x => x.IsCurrent).UserAgent, Is.EqualTo(OidcFlowRunner.BrowserUserAgent));
    }

    [Test]
    public async Task RevokedAppSessionRejectsAccessAndRefreshTokensImmediately()
    {
        var tokens = await flowRunner.RunAuthorizationCodeFlow(Email);
        using var bearerClient = flowRunner.CreateBearerClient(tokens.AccessToken);
        var responseBeforeRevocation = await bearerClient.GetAsync("api/sessions");
        Assert.That(responseBeforeRevocation.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var browser = await flowRunner.CreateLoggedInBrowser(Email);
        var appSession = (await GetSessions(browser)).Single(x => x.Type == SessionType.App);

        var revokeResponse = await browser.DeleteAsync($"api/sessions/app/{appSession.Id}");

        Assert.That(revokeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseAfterRevocation = await bearerClient.GetAsync("api/sessions");
        Assert.That(responseAfterRevocation.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var refreshResponse = await flowRunner.Refresh(tokens.RefreshToken);
        Assert.That(refreshResponse.IsSuccessStatusCode, Is.False);
    }

    [Test]
    public async Task RefreshedTokensStayInSameSession()
    {
        var tokens = await flowRunner.RunAuthorizationCodeFlow(Email);
        var refreshResponse = await flowRunner.Refresh(tokens.RefreshToken);
        Assert.That(refreshResponse.IsSuccessStatusCode, Is.True);
        using var browser = await flowRunner.CreateLoggedInBrowser(Email);

        var sessions = await GetSessions(browser);

        Assert.That(sessions.Count(x => x.Type == SessionType.App), Is.EqualTo(1));
    }

    [Test]
    public async Task RevokedWebSessionCookieIsRejected()
    {
        using var revokedBrowser = await flowRunner.CreateLoggedInBrowser(Email);
        using var otherBrowser = await flowRunner.CreateLoggedInBrowser(Email);
        var revokedSession = (await GetSessions(revokedBrowser)).Single(x => x.Type == SessionType.Web && x.IsCurrent);

        var revokeResponse = await otherBrowser.DeleteAsync($"api/sessions/web/{revokedSession.Id}");

        Assert.That(revokeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseAfterRevocation = await revokedBrowser.GetAsync("api/sessions");
        Assert.That(responseAfterRevocation.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        var otherResponse = await otherBrowser.GetAsync("api/sessions");
        Assert.That(otherResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task RevokeAllEndsSessionsOfAllAccountsOfPerson()
    {
        var tokens = await flowRunner.RunAuthorizationCodeFlow(SecondEmail);
        using var secondAccountBrowser = await flowRunner.CreateLoggedInBrowser(SecondEmail);
        using var otherPersonBrowser = await flowRunner.CreateLoggedInBrowser(OtherPersonEmail);
        using var browser = await flowRunner.CreateLoggedInBrowser(Email);

        var revokeResponse = await browser.PostAsync("api/sessions/revoke-all", null);

        Assert.That(revokeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await browser.GetAsync("api/sessions")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That((await secondAccountBrowser.GetAsync("api/sessions")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using var bearerClient = flowRunner.CreateBearerClient(tokens.AccessToken);
        Assert.That((await bearerClient.GetAsync("api/sessions")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That((await otherPersonBrowser.GetAsync("api/sessions")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task SessionsOfOtherPersonCannotBeRevoked()
    {
        await flowRunner.RunAuthorizationCodeFlow(OtherPersonEmail);
        using var otherPersonBrowser = await flowRunner.CreateLoggedInBrowser(OtherPersonEmail);
        var otherSessions = await GetSessions(otherPersonBrowser);
        using var browser = await flowRunner.CreateLoggedInBrowser(Email);

        foreach (var session in otherSessions)
        {
            var type = session.Type == SessionType.App ? "app" : "web";
            var response = await browser.DeleteAsync($"api/sessions/{type}/{session.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
        Assert.That((await otherPersonBrowser.GetAsync("api/sessions")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    private static async Task<List<SessionViewModel>> GetSessions(HttpClient client)
    {
        var response = await client.GetAsync("api/sessions");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<SessionViewModel>>(json);
    }
}
