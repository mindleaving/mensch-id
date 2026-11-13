using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Controllers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Test.TestInfrastructure;
using Mensch.Id.API.Test.Workflow;
using Mensch.Id.API.ViewModels;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Email;
using Mensch.Id.API.Workflow.ViewModelBuilders;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Controllers;

public class AccountsControllerTest
{
    private const string Password = "GJMRtmSZbH6C4p4d";
    private AccountsController controller;
    private InMemoryAccountStore accountStore;
    private Mock<IAuthenticationModule> authenticationModule;
    private Mock<IEmailSender> emailSender;
    private DefaultHttpContext httpContext;
    private VerificationEmail sentVerificationEmail;
    private PasswordResetEmail sentPasswordResetEmail;

    [SetUp]
    public void Setup()
    {
        var obscurer = TestKeys.CreateObscurer();
        accountStore = new InMemoryAccountStore(obscurer);
        authenticationModule = new Mock<IAuthenticationModule>();
        var accountCreator = new AccountCreator(accountStore, obscurer, new PasswordHasher<IAccountWithPassword>());
        emailSender = new Mock<IEmailSender>();
        emailSender.Setup(x => x.SendVerificationEmail(It.IsAny<VerificationEmail>()))
            .Callback<VerificationEmail>(x => sentVerificationEmail = x)
            .Returns(Task.CompletedTask);
        emailSender.Setup(x => x.SendPasswordResetEmail(It.IsAny<PasswordResetEmail>()))
            .Callback<PasswordResetEmail>(x => sentPasswordResetEmail = x)
            .Returns(Task.CompletedTask);
        var providerRegistry = new OidcProviderRegistry(LoginProviderViewModelBuilderTest.CreateProviders());
        var redirectUrlValidator = new RedirectUrlValidator(new RedirectUrlValidatorOptions { AllowedOrigins = ["https://mensch.id"] });
        httpContext = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };
        controller = new AccountsController(
            accountStore,
            httpContextAccessor,
            authenticationModule.Object,
            accountCreator,
            emailSender.Object,
            new AccountTokenService(new EphemeralDataProtectionProvider()),
            providerRegistry,
            new LoginProviderViewModelBuilder(providerRegistry),
            redirectUrlValidator)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static int? GetStatusCode(IActionResult result)
    {
        return ((IStatusCodeActionResult)result).StatusCode;
    }

    [Test]
    public async Task InvalidAccountTypeForLocalAccountReturnsBadRequest()
    {
        var body = new RegistrationInformation
        {
            AccountType = AccountType.External,
            Email = "joe@example.org",
            Password = Password,
            PreferedLanguage = Language.es
        };

        var result = await controller.RegisterLocalAccount(body);

        Assert.That(GetStatusCode(result), Is.EqualTo(400));
    }

    [Test]
    [TestCase(AccountType.Assigner)]
    [TestCase(AccountType.Admin)]
    public async Task ProfessionalAccountsCannotBeRegistered(AccountType accountType)
    {
        var body = new RegistrationInformation
        {
            AccountType = accountType,
            Email = "joe@example.org",
            Password = Password
        };

        var result = await controller.RegisterLocalAccount(body);

        Assert.That(GetStatusCode(result), Is.EqualTo(403));
        Assert.That(accountStore.Items, Is.Empty);
    }

    [Test]
    public async Task CreatingAccountWhileSignedInLinksAccount()
    {
        var body = new RegistrationInformation
        {
            AccountType = AccountType.Local,
            Email = "joe@example.org",
            Password = Password,
            PreferedLanguage = Language.es
        };
        const string PersonId = "20220609-XUTH9";
        var claims = new List<Claim>
        {
            new(MenschIdClaimTypes.PersonIdClaimName, PersonId, null, JwtSecurityTokenBuilder.Issuer)
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = await controller.RegisterLocalAccount(body);

        var account = accountStore.Items.Single();
        Assert.That(result, Is.TypeOf<OkResult>());
        Assert.That(account, Is.TypeOf<LocalAccount>());
        Assert.That(((LocalAccount)account).PersonId, Is.EqualTo(PersonId));
    }

    [Test]
    public async Task RegistrationDoesNotCreateMenschId()
    {
        var body = new RegistrationInformation
        {
            AccountType = AccountType.LocalAnonymous,
            Password = Password
        };
        authenticationModule
            .Setup(x => x.BuildSecurityTokenForUser(It.IsAny<LocalAnonymousAccount>()))
            .Returns(AuthenticationResult.Success([], "token", AccountType.LocalAnonymous));

        await controller.RegisterLocalAccount(body);

        var account = (LocalAnonymousAccount)accountStore.Items.Single();
        Assert.That(account.PersonId, Is.Null);
    }

    [Test]
    public async Task EmailCanBeVerifiedWithTokenFromEmailExactlyOnce()
    {
        var body = new RegistrationInformation
        {
            AccountType = AccountType.Local,
            Email = "joe@example.org",
            Password = Password
        };
        await controller.RegisterLocalAccount(body);
        var accountId = sentVerificationEmail.AccountId;

        var firstResult = await controller.VerifyEmail(accountId, sentVerificationEmail.VerificationToken);
        var account = (LocalAccount)await accountStore.GetByIdAsync(accountId);

        Assert.That(firstResult, Is.TypeOf<OkResult>());
        Assert.That(account.IsEmailVerified, Is.True);
        Assert.That(account.EmailVerificationNonce, Is.Null);
    }

    [Test]
    public async Task InvalidVerificationTokenIsRejected()
    {
        var body = new RegistrationInformation
        {
            AccountType = AccountType.Local,
            Email = "joe@example.org",
            Password = Password
        };
        await controller.RegisterLocalAccount(body);
        var accountId = sentVerificationEmail.AccountId;

        var result = await controller.VerifyEmail(accountId, "invalid-token");

        var account = (LocalAccount)await accountStore.GetByIdAsync(accountId);
        Assert.That(GetStatusCode(result), Is.EqualTo(403));
        Assert.That(account.IsEmailVerified, Is.False);
    }

    [Test]
    public async Task PasswordResetTokenIsSingleUse()
    {
        var account = new LocalAccount { Id = "account-1", Email = "joe@example.org", IsEmailVerified = true };
        await accountStore.StoreAsync(account);
        authenticationModule
            .Setup(x => x.ChangePasswordAsync("account-1", It.IsAny<string>(), false))
            .ReturnsAsync(true)
            .Callback(() => account.PasswordResetNonce = null);
        authenticationModule
            .Setup(x => x.AuthenticateLocalByEmailMenschIdOrUsernameAsync(It.IsAny<LoginInformation>()))
            .ReturnsAsync(AuthenticationResult.Success([], "token", AccountType.Local));
        await controller.RequestPasswordResetEmail(new ResetPasswordRequest { Email = "joe@example.org" });
        var resetBody = new ResetPasswordBody
        {
            AccountId = "account-1",
            ResetToken = sentPasswordResetEmail.ResetToken,
            Password = "NewPassword123"
        };

        var firstResult = await controller.ResetPassword("account-1", resetBody);
        var secondResult = await controller.ResetPassword("account-1", resetBody);

        Assert.That(GetStatusCode(firstResult), Is.EqualTo(200));
        Assert.That(GetStatusCode(secondResult), Is.EqualTo(403));
    }

    [Test]
    public async Task PasswordResetTokenOfOtherAccountIsRejected()
    {
        await accountStore.StoreAsync(new LocalAccount { Id = "account-1", Email = "joe@example.org", IsEmailVerified = true });
        await accountStore.StoreAsync(new LocalAccount { Id = "account-2", Email = "jane@example.org", IsEmailVerified = true });
        await controller.RequestPasswordResetEmail(new ResetPasswordRequest { Email = "joe@example.org" });
        var tokenOfAccount1 = sentPasswordResetEmail.ResetToken;
        await controller.RequestPasswordResetEmail(new ResetPasswordRequest { Email = "jane@example.org" });
        var resetBody = new ResetPasswordBody
        {
            AccountId = "account-2",
            ResetToken = tokenOfAccount1,
            Password = "NewPassword123"
        };

        var result = await controller.ResetPassword("account-2", resetBody);

        Assert.That(GetStatusCode(result), Is.EqualTo(403));
        authenticationModule.Verify(x => x.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void LoginProvidersAreReturnedWithoutSecrets()
    {
        var result = controller.GetLoginProviders();

        var viewModels = (List<LoginProviderViewModel>)((OkObjectResult)result).Value!;
        var json = JsonConvert.SerializeObject(viewModels);
        Assert.That(viewModels.Select(x => x.Id.Value), Is.EqualTo(new[] { "google", "eidas" }));
        Assert.That(json, Does.Not.Contain(LoginProviderViewModelBuilderTest.ClientSecret));
        Assert.That(json, Does.Not.Contain(LoginProviderViewModelBuilderTest.ClientId));
        Assert.That(json, Does.Not.Contain("authority.example.org"));
    }

    [Test]
    public async Task UnknownLoginProviderIsRejected()
    {
        var result = await controller.ExternalLogin(new LoginProvider("facebook"));

        Assert.That(GetStatusCode(result), Is.EqualTo(400));
    }

    [Test]
    public async Task ForeignRedirectUrlIsRejected()
    {
        var result = await controller.ExternalLogin(new LoginProvider("google"), "https://evil.example.org/steal");

        Assert.That(GetStatusCode(result), Is.EqualTo(400));
    }

    [Test]
    public async Task KnownLoginProviderIssuesChallengeWithRedirect()
    {
        var result = await controller.ExternalLogin(new LoginProvider("google"), "https://mensch.id/login/redirect");

        var challengeResult = (ChallengeResult)result;
        Assert.That(challengeResult.AuthenticationSchemes, Is.EqualTo(new[] { "oidc-google" }));
        Assert.That(challengeResult.Properties!.RedirectUri, Is.EqualTo("https://mensch.id/login/redirect"));
    }
}
