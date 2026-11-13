using Mensch.Id.API.AccessControl;
using NUnit.Framework;

namespace Mensch.Id.API.Test.AccessControl;

public class RedirectUrlValidatorTest
{
    private RedirectUrlValidator validator;

    [SetUp]
    public void Setup()
    {
        var options = new RedirectUrlValidatorOptions
        {
            AllowedOrigins = ["https://mensch.id", "https://localhost:3301/"]
        };
        validator = new RedirectUrlValidator(options);
    }

    [Test]
    [TestCase("/")]
    [TestCase("/login/redirect")]
    [TestCase("https://mensch.id/login/redirect")]
    [TestCase("https://MENSCH.ID/linkaccount/finish")]
    [TestCase("https://localhost:3301/login/redirect")]
    public void AllowedUrlsAreAccepted(string redirectUrl)
    {
        Assert.That(validator.IsAllowed(redirectUrl), Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("//evil.example.org/path")]
    [TestCase("/\\evil.example.org")]
    [TestCase("https://evil.example.org/login/redirect")]
    [TestCase("https://mensch.id.evil.example.org/")]
    [TestCase("http://mensch.id/login/redirect")]
    [TestCase("https://localhost:3302/login/redirect")]
    [TestCase("javascript:alert(1)")]
    [TestCase("login/redirect")]
    public void DisallowedUrlsAreRejected(string redirectUrl)
    {
        Assert.That(validator.IsAllowed(redirectUrl), Is.False);
    }
}
