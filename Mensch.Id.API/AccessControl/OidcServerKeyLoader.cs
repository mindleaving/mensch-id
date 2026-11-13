using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Mensch.Id.API.AccessControl;

public static class OidcServerKeyLoader
{
    public static SecurityKey LoadSigningKey(string pemPath)
    {
        var pem = File.ReadAllText(pemPath);
        if (TryLoadRsa(pem, out var rsaKey))
            return rsaKey;
        var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(pem);
        return new ECDsaSecurityKey(ecdsa);
    }

    public static SecurityKey LoadEncryptionKey(string pemPath)
    {
        var pem = File.ReadAllText(pemPath);
        if (TryLoadRsa(pem, out var rsaKey))
            return rsaKey;
        throw new CryptographicException($"Encryption key '{pemPath}' must be an RSA key");
    }

    private static bool TryLoadRsa(
        string pem,
        out RsaSecurityKey rsaKey)
    {
        var rsa = RSA.Create();
        try
        {
            rsa.ImportFromPem(pem);
            rsaKey = new RsaSecurityKey(rsa);
            return true;
        }
        catch (CryptographicException)
        {
            rsa.Dispose();
            rsaKey = null;
            return false;
        }
    }
}
