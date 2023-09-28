using Mensch.Id.API.Models;
using Mensch.Id.API.Models.AccessControl;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Mensch.Id.API.Test.AccessControl
{
    public class PasswordHasherTest
    {
        [Test]
        public void AspNetIdentityPasswordHasherSupportsLocalAccount()
        {
            var passwordHasher = new PasswordHasher<LocalAnonymousAccount>();
            var user = new LocalAnonymousAccount();
            var hashedPassword = passwordHasher.HashPassword(user, "password");
            var verificationResult = passwordHasher.VerifyHashedPassword(user, hashedPassword, "password");
            Assert.That(verificationResult == PasswordVerificationResult.Success);
        }

        [Test]
        public void IncrementOfPasswordHasherIterationsReturnsRehashNotice()
        {
            var oldOptions = new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = 100_000
            };
            var oldPasswordHasher = new PasswordHasher<LocalAnonymousAccount>(new OptionsWrapper<PasswordHasherOptions>(oldOptions));
            var user = new LocalAnonymousAccount();
            var hashedPassword = oldPasswordHasher.HashPassword(user, "password");
            var oldVerificationResult = oldPasswordHasher.VerifyHashedPassword(user, hashedPassword, "password");
            Assume.That(oldVerificationResult == PasswordVerificationResult.Success);

            var newOptions = new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = 120_000
            };
            var newPasswordHasher = new PasswordHasher<LocalAnonymousAccount>(new OptionsWrapper<PasswordHasherOptions>(newOptions));
            var newVerificationResult = newPasswordHasher.VerifyHashedPassword(user, hashedPassword, "password");
            Assert.That(newVerificationResult == PasswordVerificationResult.SuccessRehashNeeded);
        }
    }
}
