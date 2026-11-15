using System;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    internal class SaltGenerator
    {
        [Test]
        public void GenerateSalt()
        {
            var salt = RandomNumberGenerator.GetBytes(32);
            Console.WriteLine(Convert.ToBase64String(salt));
        }

        [Test]
        public void GeneratePassword()
        {
            var password = RandomNumberGenerator.GetString("abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789!.,-_%&#?", 24);
            Console.WriteLine(password);
        }
    }
}
