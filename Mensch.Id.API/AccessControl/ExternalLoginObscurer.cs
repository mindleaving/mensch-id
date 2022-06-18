using System;

namespace Mensch.Id.API.AccessControl
{
    public class ExternalLoginObscurer : IExternalLoginObscurer
    {
        private readonly string salt;
        private readonly string secretPrefix;

        public ExternalLoginObscurer(
            string salt,
            string secretPrefix)
        {
            this.salt = salt;
            this.secretPrefix = secretPrefix;
        }

        public string Obscure(string accountId)
        {
            var obscuredAccountIdBytes = PasswordHasher.Hash(secretPrefix + accountId, salt);
            return Convert.ToBase64String(obscuredAccountIdBytes);
        }
    }
}
