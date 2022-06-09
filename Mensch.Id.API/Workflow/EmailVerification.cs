using System;
using Mensch.Id.API.AccessControl;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow
{
    public static class EmailVerification
    {
        public static string GenerateToken(byte[] salt, out string unencryptedToken)
        {
            unencryptedToken = new TemporaryPasswordGenerator().Generate(length: 32);
            return Convert.ToBase64String(PasswordHasher.Hash(unencryptedToken, salt));
        }
        public static string GenerateToken(string salt, out string unencryptedToken) => GenerateToken(Convert.FromBase64String(salt), out unencryptedToken);

        public static bool Verify(string token, LocalAccount account)
        {
            if (token == null || account.PasswordResetToken == null)
                return false;
            return HashComparer.Compare(token, account.EmailVerificationToken, account.Salt);
        }
    }
}
