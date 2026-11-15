using System.ComponentModel;
using Mensch.Id.API.Models;
using NUnit.Framework;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using JsonSerializationException = Newtonsoft.Json.JsonSerializationException;

namespace Mensch.Id.API.Test.Models;

public class OwnershipSecretTest
{
    private const string ValidSecret = "ACDEFGHJKLMNPQRSTUVWXYZ2345679";

    private class OwnershipSecretContainer
    {
        public OwnershipSecret? Secret { get; set; }
    }

    [Test]
    public void ValidSecretIsAccepted()
    {
        var secret = new OwnershipSecret(ValidSecret);

        Assert.That(secret.Value, Is.EqualTo(ValidSecret));
        Assert.That(OwnershipSecret.IsValid(ValidSecret), Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("ACDEFGHJKLMNPQRSTUVWXYZ234567")]
    [TestCase("ACDEFGHJKLMNPQRSTUVWXYZ23456799")]
    [TestCase("acdefghjklmnpqrstuvwxyz2345679")]
    [TestCase("ACDEFG-HJKLMN-PQRSTU-VWXYZ2-345679")]
    [TestCase("BCDEFGHJKLMNPQRSTUVWXYZ2345679")]
    [TestCase("ACDEFGHJKLMNPQRSTUVWXYZ2345678")]
    [TestCase(" ACDEFGHJKLMNPQRSTUVWXYZ2345679")]
    public void InvalidSecretsAreRejected(string input)
    {
        Assert.That(() => new OwnershipSecret(input), Throws.ArgumentException);
        Assert.That(OwnershipSecret.IsValid(input), Is.False);
    }

    [Test]
    public void GeneratedSecretsAreValidAndUnique()
    {
        var first = OwnershipSecret.Generate();
        var second = OwnershipSecret.Generate();

        Assert.That(OwnershipSecret.IsValid(first.Value), Is.True);
        Assert.That(first.Value, Has.Length.EqualTo(OwnershipSecret.Length));
        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void EqualityOperatorsCompareValues()
    {
        var first = new OwnershipSecret(ValidSecret);
        var second = new OwnershipSecret(ValidSecret);
        var other = new OwnershipSecret(new string('A', OwnershipSecret.Length));

        Assert.That(first == second, Is.True);
        Assert.That(first != second, Is.False);
        Assert.That(first == other, Is.False);
        Assert.That(first != other, Is.True);
        Assert.That(first.Equals((object)second), Is.True);
        Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        Assert.That(default(OwnershipSecret) == default(OwnershipSecret), Is.True);
        Assert.That(default(OwnershipSecret) == first, Is.False);
    }

    [Test]
    public void JsonRoundtripUsesPlainString()
    {
        var container = new OwnershipSecretContainer { Secret = new OwnershipSecret(ValidSecret) };

        var json = JsonConvert.SerializeObject(container);
        var deserialized = JsonConvert.DeserializeObject<OwnershipSecretContainer>(json);

        Assert.That(json, Is.EqualTo($"{{\"Secret\":\"{ValidSecret}\"}}"));
        Assert.That(deserialized.Secret, Is.EqualTo(container.Secret));
    }

    [Test]
    public void JsonDeserializationRejectsInvalidValue()
    {
        Assert.That(
            () => JsonConvert.DeserializeObject<OwnershipSecretContainer>("{\"Secret\":\"too-short\"}"),
            Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TypeConverterRejectsInvalidValue()
    {
        var converter = TypeDescriptor.GetConverter(typeof(OwnershipSecret));

        var secret = (OwnershipSecret)converter.ConvertFrom(ValidSecret)!;

        Assert.That(secret.Value, Is.EqualTo(ValidSecret));
        Assert.That(() => converter.ConvertFrom("invalid"), Throws.ArgumentException);
    }
}
