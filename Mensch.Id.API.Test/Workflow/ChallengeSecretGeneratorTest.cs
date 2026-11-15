using System;
using Mensch.Id.API.Workflow;
using NUnit.Framework;

namespace Mensch.Id.API.Test.Workflow
{
    internal class ChallengeSecretGeneratorTest
    {
        [Test]
        public void NonPositiveChallengeLengthThrowsException()
        {
            Assert.That(() => ChallengeSecretGenerator.Generate(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => ChallengeSecretGenerator.Generate(0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ChallengeSecretHasExpectedLength()
        {
            const int Length = 10;
            var actual = ChallengeSecretGenerator.Generate(Length);

            Assert.That(actual, Has.Length.EqualTo(Length));
        }

        [Test]
        public void ChallengeSecretNotEmpty()
        {
            var actual = ChallengeSecretGenerator.Generate();

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.Not.Empty);
        }

        [Test]
        public void GeneratedSecretIsValid()
        {
            var secret = ChallengeSecretGenerator.Generate(52);

            Assert.That(ChallengeSecretGenerator.IsValid(secret, 52), Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase("ABC")]
        [TestCase("ABCDEFGHI0")]
        [TestCase("abcdefghjk")]
        public void InvalidSecretsAreRejected(string secret)
        {
            Assert.That(ChallengeSecretGenerator.IsValid(secret, 10), Is.False);
        }
    }
}
