using System;
using System.Collections.Generic;
using System.ComponentModel;
using Mensch.Id.API.Models;
using NUnit.Framework;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using JsonSerializationException = Newtonsoft.Json.JsonSerializationException;

namespace Mensch.Id.API.Test.Models;

public class MenschIdTest
{
    private class MenschIdContainer
    {
        public MenschId? Id { get; set; }
    }

    [Test]
    [TestCase("20000101-ACDEF", "20000101-ACDEF")]
    [TestCase("20000101-acdef", "20000101-ACDEF")]
    [TestCase(" 19991231-Z2345 ", "19991231-Z2345")]
    [TestCase("20000101-ACDEF\n", "20000101-ACDEF")]
    public void ValidValuesAreAcceptedAndNormalized(string input, string expected)
    {
        var menschId = new MenschId(input);

        Assert.That(menschId.Value, Is.EqualTo(expected));
        Assert.That(MenschId.IsValid(input), Is.True);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("2000010-ACDEF")]
    [TestCase("20000101ACDEF")]
    [TestCase("20000101-ACDE")]
    [TestCase("20000101-ACDEFG")]
    [TestCase("x20000101-ACDEF")]
    [TestCase("20000101-ACDEB")]
    [TestCase("20000101-ACDE1")]
    [TestCase("20000101-ACDE8")]
    [TestCase("../../etc/passwd")]
    public void InvalidValuesAreRejected(string input)
    {
        Assert.That(() => new MenschId(input), Throws.ArgumentException);
        Assert.That(MenschId.IsValid(input), Is.False);
    }

    [Test]
    public void EqualityOperatorsCompareNormalizedValues()
    {
        var first = new MenschId("20000101-ACDEF");
        var second = new MenschId("20000101-acdef");
        var other = new MenschId("20000101-ACDEG");

        Assert.That(first == second, Is.True);
        Assert.That(first != second, Is.False);
        Assert.That(first == other, Is.False);
        Assert.That(first != other, Is.True);
        Assert.That(first.Equals((object)second), Is.True);
        Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        Assert.That(new HashSet<MenschId> { first, second }, Has.Count.EqualTo(1));
    }

    [Test]
    public void JsonRoundtripUsesPlainString()
    {
        var container = new MenschIdContainer { Id = new MenschId("20000101-ACDEF") };

        var json = JsonConvert.SerializeObject(container);
        var deserialized = JsonConvert.DeserializeObject<MenschIdContainer>(json);

        Assert.That(json, Is.EqualTo("{\"Id\":\"20000101-ACDEF\"}"));
        Assert.That(deserialized.Id, Is.EqualTo(container.Id));
    }

    [Test]
    public void JsonNullDeserializesToNull()
    {
        var deserialized = JsonConvert.DeserializeObject<MenschIdContainer>("{\"Id\":null}");

        Assert.That(deserialized.Id, Is.Null);
    }

    [Test]
    public void JsonDeserializationRejectsInvalidValue()
    {
        Assert.That(
            () => JsonConvert.DeserializeObject<MenschIdContainer>("{\"Id\":\"not-a-mensch-id\"}"),
            Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TypeConverterConvertsFromAndToString()
    {
        var converter = TypeDescriptor.GetConverter(typeof(MenschId));

        var menschId = (MenschId)converter.ConvertFrom("20000101-acdef")!;

        Assert.That(menschId.Value, Is.EqualTo("20000101-ACDEF"));
        Assert.That(converter.ConvertToString(menschId), Is.EqualTo("20000101-ACDEF"));
        Assert.That(() => converter.ConvertFrom("invalid"), Throws.ArgumentException);
    }

    [Test]
    public void GeneratedIdsAreValid()
    {
        var generator = new Mensch.Id.API.Workflow.MenschIdGenerator();

        for (var index = 0; index < 100; index++)
        {
            var generatedId = generator.Generate(new DateOnly(2000, 1, 1));
            Assert.That(MenschId.IsValid(generatedId), Is.True, generatedId);
        }
    }
}
