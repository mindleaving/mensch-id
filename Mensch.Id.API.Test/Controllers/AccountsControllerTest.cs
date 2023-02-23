using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Controllers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow;
using Mensch.Id.API.Workflow.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Controllers
{
    public class AccountsControllerTest
    {
        private AccountsController controller;
        private Mock<IAccountStore> accountStore;
        private Mock<IHttpContextAccessor> httpContextAccessor;
        private Mock<IAuthenticationModule> authenticationModule;
        private AccountCreator accountCreator;
        private Mock<IEmailSender> emailSender;
        private Mock<IExternalLoginObscurer> externalLoginObscurer;
        private Mock<IPasswordHasher<LocalAnonymousAccount>> passwordHasher;

        [SetUp]
        public void Setup()
        {
            accountStore = new Mock<IAccountStore>();
            httpContextAccessor = new Mock<IHttpContextAccessor>();
            authenticationModule = new Mock<IAuthenticationModule>();
            externalLoginObscurer = new Mock<IExternalLoginObscurer>();
            passwordHasher = new Mock<IPasswordHasher<LocalAnonymousAccount>>();
            accountCreator = new AccountCreator(accountStore.Object, externalLoginObscurer.Object, passwordHasher.Object);
            emailSender = new Mock<IEmailSender>();
            controller = new AccountsController(
                accountStore.Object,
                httpContextAccessor.Object,
                authenticationModule.Object,
                accountCreator,
                emailSender.Object);
        }

        [Test]
        public async Task InvalidAccountTypeForLocalAccountReturnsBadRequest()
        {
            // Arrange
            var body = new RegistrationInformation
            {
                AccountType = AccountType.External,
                Email = "joe@example.org",
                Password = "GJMRtmSZbH6C4p4d",
                PreferedLanguage = Language.es
            };

            // Act
            var result = await controller.RegisterLocalAccount(body);

            // Assert
            Assert.That(result, Is.InstanceOf<IStatusCodeActionResult>());
            Assert.That(((IStatusCodeActionResult)result).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task CreatingAccountWhileSignedInLinksAccount()
        {
            // Arrange
            var body = new RegistrationInformation
            {
                AccountType = AccountType.Local,
                Email = "joe@example.org",
                Password = "GJMRtmSZbH6C4p4d",
                PreferedLanguage = Language.es
            };
            var personId = "20220609-XUTH9";
            var claims = new List<Claim>
            {
                new(MenschIdClaimTypes.PersonIdClaimName, personId, null, JwtSecurityTokenBuilder.Issuer)
            };
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            };
            httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);
            Account account = null;
            accountStore.Setup(x => x.StoreAsync(It.IsAny<Account>())).Callback<Account>(x => account = x);

            // Act
            var result = await controller.RegisterLocalAccount(body);

            // Assert
            Assert.That(result, Is.TypeOf<OkResult>());
            accountStore.Verify(x => x.StoreAsync(It.IsAny<LocalAccount>()), Times.Once);
            Assert.That(account, Is.Not.Null);
            Assert.That(account, Is.TypeOf<LocalAccount>());
            Assert.That(account.PersonId, Is.EqualTo(personId));
        }
    }
}