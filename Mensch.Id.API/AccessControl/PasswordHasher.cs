using System;
using System.Security.Cryptography;

namespace Mensch.Id.API.AccessControl
{
    public static class PasswordHasher
    {
        public const int RecommendedHashLength = 256;


        public static byte[] Hash(string password, string salt, int hashBitLength = RecommendedHashLength) 
            => Hash(password, Convert.FromBase64String(salt), hashBitLength);

        public static byte[] Hash(string password, byte[] salt, int hashBitLength = RecommendedHashLength)
        {
            if(hashBitLength < 128)
                throw new ArgumentException("Requested hash length is too short. " +
                                            "Technically there isn't anything wrong with the requested hash length, " +
                                            "but I'm worried that you might not know why a long hash is important. " +
                                            "If you absolutely need a shorter hash length, truncate the returned byte array.");
            const int Iterations = 10000;
            using var kdf = new Rfc2898DeriveBytes(password, salt, Iterations);
            return kdf.GetBytes(hashBitLength / 8);
        }

        public static byte[] CreateSalt(int hashBitLength = 128)
        {
            if (hashBitLength % 8 != 0)
                throw new ArgumentOutOfRangeException($"{nameof(hashBitLength)} must be a multiple of 8");
            var salt = new byte[hashBitLength / 8];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(salt);
            return salt;
        }
    }
}