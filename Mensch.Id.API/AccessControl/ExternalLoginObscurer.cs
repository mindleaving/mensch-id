using System;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.AccessControl;

public class ExternalLoginObscurer : IExternalLoginObscurer
{
    public const int MinimumKeyLengthInBytes = 32;
    private readonly byte[] key;

    public ExternalLoginObscurer(byte[] key)
    {
        if (key == null || key.Length < MinimumKeyLengthInBytes)
            throw new ArgumentException($"Key for obscuring external logins must be at least {MinimumKeyLengthInBytes} bytes long", nameof(key));
        this.key = key.ToArray();
    }

    public string Obscure(
        LoginProvider loginProvider,
        string externalId)
    {
        if (string.IsNullOrEmpty(externalId))
            throw new ArgumentException("External ID must not be empty", nameof(externalId));
        var input = Encoding.UTF8.GetBytes($"{loginProvider.Value}:{externalId}");
        var hash = HMACSHA256.HashData(key, input);
        return Convert.ToBase64String(hash);
    }
}
