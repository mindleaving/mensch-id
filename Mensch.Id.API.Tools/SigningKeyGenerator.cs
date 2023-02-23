using System;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    internal class SigningKeyGenerator
    {
        [Test]
        public void GenerateRSA()
        {
            using var rsa = RSA.Create();
            var privateKey = rsa.ExportRSAPrivateKey();
            Console.WriteLine(Convert.ToBase64String(privateKey));
        }

        [Test]
        public void GenerateECDSA()
        {
            using var ecDsa = ECDsa.Create();
            var privateKey = ecDsa.ExportECPrivateKey();
            Console.WriteLine(Convert.ToBase64String(privateKey));
        }
    }
}
