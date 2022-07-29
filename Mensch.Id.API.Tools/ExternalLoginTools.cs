using System;
using Mensch.Id.API.AccessControl;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    internal class ExternalLoginTools
    {
        [Test]
        [TestCase("")]
        public void ObscureExternalId(string externalId)
        {
            const string Salt = "";
            const string Secret = "";
            var obscurer = new ExternalLoginObscurer(Salt, Secret);
            var obscuredExternalId = obscurer.Obscure(externalId);
            Console.WriteLine(obscuredExternalId);
        }
    }
}
