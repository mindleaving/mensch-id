using System;

namespace Mensch.Id.API.AccessControl
{
    public static class HashComparer
    {
        public static bool Compare(
            string passwordCandidate,
            string storedPasswordHash,
            string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            var storedPasswordHashBytes = Convert.FromBase64String(storedPasswordHash);
            var providedPasswordHashBytes = PasswordHasher.Hash(passwordCandidate, saltBytes, 8 * storedPasswordHashBytes.Length);
            return Compare(providedPasswordHashBytes, storedPasswordHashBytes);
        }

        /// <summary>
        /// Constant-time compares to byte arrays
        /// </summary>
        public static bool Compare(
            byte[] a,
            byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            var areEqual = true;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    areEqual = false;
            }
            return areEqual;
        }
    }
}
