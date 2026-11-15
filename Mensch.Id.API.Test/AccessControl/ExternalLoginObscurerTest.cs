using System;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using Mensch.Id.API.Test.TestInfrastructure;
using NUnit.Framework;

namespace Mensch.Id.API.Test.AccessControl;

public class ExternalLoginObscurerTest
{
    private static readonly LoginProvider Google = new("google");
    private static readonly LoginProvider Microsoft = new("microsoft");

    [Test]
    public void ObscuringIsDeterministic()
    {
        var obscurer = TestKeys.CreateObscurer();

        var first = obscurer.Obscure(Google, "12345");
        var second = obscurer.Obscure(Google, "12345");

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void ObscuredValueDoesNotContainExternalId()
    {
        var obscurer = TestKeys.CreateObscurer();

        var obscured = obscurer.Obscure(Google, "user-12345");

        Assert.That(obscured, Does.Not.Contain("12345"));
        Assert.That(Convert.FromBase64String(obscured), Has.Length.EqualTo(32));
    }

    [Test]
    public void SameExternalIdOfDifferentProvidersResultsInDifferentValues()
    {
        var obscurer = TestKeys.CreateObscurer();

        var googleValue = obscurer.Obscure(Google, "12345");
        var microsoftValue = obscurer.Obscure(Microsoft, "12345");

        Assert.That(googleValue, Is.Not.EqualTo(microsoftValue));
    }

    [Test]
    public void DifferentKeysResultInDifferentValues()
    {
        var otherKey = new byte[32];
        var obscurer = TestKeys.CreateObscurer();
        var otherObscurer = new ExternalLoginObscurer(otherKey);

        var value = obscurer.Obscure(Google, "12345");
        var otherValue = otherObscurer.Obscure(Google, "12345");

        Assert.That(value, Is.Not.EqualTo(otherValue));
    }

    [Test]
    public void ShortKeyIsRejected()
    {
        var shortKey = new byte[ExternalLoginObscurer.MinimumKeyLengthInBytes - 1];

        Assert.That(() => new ExternalLoginObscurer(shortKey), Throws.ArgumentException);
        Assert.That(() => new ExternalLoginObscurer(null), Throws.ArgumentException);
    }

    [Test]
    public void EmptyExternalIdIsRejected()
    {
        var obscurer = TestKeys.CreateObscurer();

        Assert.That(() => obscurer.Obscure(Google, ""), Throws.ArgumentException);
        Assert.That(() => obscurer.Obscure(Google, null), Throws.ArgumentException);
    }

    [Test]
    public void ChangingKeyArrayAfterConstructionDoesNotAffectObscurer()
    {
        var key = (byte[])TestKeys.ObscureKey.Clone();
        var obscurer = new ExternalLoginObscurer(key);
        var before = obscurer.Obscure(Google, "12345");

        key[0] ^= 0xFF;
        var after = obscurer.Obscure(Google, "12345");

        Assert.That(after, Is.EqualTo(before));
    }
}
