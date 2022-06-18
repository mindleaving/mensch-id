using System;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Workflow;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    internal class SaltGenerator
    {
        [Test]
        public void GenerateSalt()
        {
            var salt = PasswordHasher.CreateSalt();
            Console.WriteLine(Convert.ToBase64String(salt));
        }

        [Test]
        public void GeneratePassword()
        {
            var password = new TemporaryPasswordGenerator().Generate(length: 24);
            Console.WriteLine(password);
        }
    }
}
