using System.Collections.Generic;
using System.ComponentModel;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Models;

public class LoginProviderTest
{
    private class LoginProviderContainer
    {
        public LoginProvider LoginProvider { get; set; }
    }

    [Test]
    [TestCase("google")]
    [TestCase("Microsoft")]
    [TestCase("eIDAS2")]
    [TestCase("a")]
    public void ValidValuesAreAccepted(string value)
    {
        var loginProvider = new LoginProvider(value);

        Assert.That(loginProvider.Value, Is.EqualTo(value));
        Assert.That(LoginProvider.IsValid(value), Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("google ")]
    [TestCase("my-provider")]
    [TestCase("my_provider")]
    [TestCase("provider.com")]
    [TestCase("prövider")]
    [TestCase("../etc")]
    [TestCase("google\n")]
    public void InvalidValuesAreRejected(string value)
    {
        Assert.That(() => new LoginProvider(value), Throws.ArgumentException);
        Assert.That(LoginProvider.IsValid(value), Is.False);
    }

    [Test]
    public void EqualityOperatorsCompareValues()
    {
        var first = new LoginProvider("google");
        var second = new LoginProvider("google");
        var other = new LoginProvider("microsoft");

        Assert.That(first == second, Is.True);
        Assert.That(first != second, Is.False);
        Assert.That(first == other, Is.False);
        Assert.That(first != other, Is.True);
        Assert.That(first.Equals(second), Is.True);
        Assert.That(first.Equals((object)second), Is.True);
        Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
    }

    [Test]
    public void ComparisonIsCaseSensitive()
    {
        var lowerCase = new LoginProvider("google");
        var upperCase = new LoginProvider("Google");

        Assert.That(lowerCase == upperCase, Is.False);
    }

    [Test]
    public void CanBeUsedAsDictionaryKey()
    {
        var dictionary = new Dictionary<LoginProvider, int>
        {
            { new LoginProvider("google"), 1 }
        };

        Assert.That(dictionary.ContainsKey(new LoginProvider("google")), Is.True);
        Assert.That(dictionary.ContainsKey(new LoginProvider("microsoft")), Is.False);
    }

    [Test]
    public void JsonRoundtripPreservesValue()
    {
        var container = new LoginProviderContainer { LoginProvider = new LoginProvider("google") };

        var json = JsonConvert.SerializeObject(container);
        var deserialized = JsonConvert.DeserializeObject<LoginProviderContainer>(json);

        Assert.That(json, Is.EqualTo("{\"LoginProvider\":\"google\"}"));
        Assert.That(deserialized.LoginProvider, Is.EqualTo(container.LoginProvider));
    }

    [Test]
    public void JsonDeserializationRejectsInvalidValue()
    {
        const string Json = "{\"LoginProvider\":\"not-valid\"}";

        Assert.That(() => JsonConvert.DeserializeObject<LoginProviderContainer>(Json), Throws.Exception);
    }

    [Test]
    public void BsonRoundtripPreservesValue()
    {
        var serializer = new LoginProviderBsonSerializer();
        var loginProvider = new LoginProvider("google");
        var document = new BsonDocument();

        using (var writer = new BsonDocumentWriter(document))
        {
            writer.WriteStartDocument();
            writer.WriteName("value");
            var serializationContext = BsonSerializationContext.CreateRoot(writer);
            serializer.Serialize(serializationContext, loginProvider);
            writer.WriteEndDocument();
        }
        LoginProvider deserialized;
        using (var reader = new BsonDocumentReader(document))
        {
            reader.ReadStartDocument();
            reader.ReadName();
            var deserializationContext = BsonDeserializationContext.CreateRoot(reader);
            deserialized = serializer.Deserialize(deserializationContext);
            reader.ReadEndDocument();
        }

        Assert.That(document["value"].BsonType, Is.EqualTo(BsonType.String));
        Assert.That(document["value"].AsString, Is.EqualTo("google"));
        Assert.That(deserialized, Is.EqualTo(loginProvider));
    }

    [Test]
    public void BsonDeserializationOfLegacyIntegerValueReturnsDefault()
    {
        var serializer = new LoginProviderBsonSerializer();
        var document = new BsonDocument("value", 1);

        LoginProvider deserialized;
        using (var reader = new BsonDocumentReader(document))
        {
            reader.ReadStartDocument();
            reader.ReadName();
            var deserializationContext = BsonDeserializationContext.CreateRoot(reader);
            deserialized = serializer.Deserialize(deserializationContext);
            reader.ReadEndDocument();
        }

        Assert.That(deserialized, Is.EqualTo(default(LoginProvider)));
    }

    [Test]
    public void TypeConverterConvertsFromAndToString()
    {
        var converter = TypeDescriptor.GetConverter(typeof(LoginProvider));

        var loginProvider = (LoginProvider)converter.ConvertFrom("google")!;
        var stringValue = converter.ConvertToString(loginProvider);

        Assert.That(loginProvider, Is.EqualTo(new LoginProvider("google")));
        Assert.That(stringValue, Is.EqualTo("google"));
        Assert.That(() => converter.ConvertFrom("in-valid"), Throws.ArgumentException);
    }

    [Test]
    public void LocalProviderIsValid()
    {
        Assert.That(LoginProvider.Local.Value, Is.EqualTo("local"));
        Assert.That(() => new LoginProvider(LoginProvider.Local.Value), Throws.Nothing);
        Assert.That(LoginProvider.Local, Is.Not.EqualTo(default(LoginProvider)));
    }

    [Test]
    public void ToStringReturnsValue()
    {
        var loginProvider = new LoginProvider("google");

        Assert.That(loginProvider.ToString(), Is.EqualTo("google"));
        Assert.That($"{loginProvider}", Is.EqualTo("google"));
    }

    [Test]
    public void DefaultValuesAreEqual()
    {
        Assert.That(default(LoginProvider) == new LoginProvider(), Is.True);
        Assert.That(() => default(LoginProvider).GetHashCode(), Throws.Nothing);
    }
}
