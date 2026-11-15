using System;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    internal class ExternalLoginTools
    {
        [Test]
        [TestCase("", "")]
        public void ObscureExternalId(
            string loginProvider,
            string externalId)
        {
            const string ObscureKeyBase64 = "";
            var obscureKey = Convert.FromBase64String(ObscureKeyBase64);
            var obscurer = new ExternalLoginObscurer(obscureKey);
            var obscuredExternalId = obscurer.Obscure(new LoginProvider(loginProvider), externalId);
            Console.WriteLine(obscuredExternalId);
        }
    }
}
