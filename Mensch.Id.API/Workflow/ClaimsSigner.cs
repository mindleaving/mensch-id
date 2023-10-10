using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System;
using Newtonsoft.Json;

namespace Mensch.Id.API.Workflow
{
    public class ClaimsSigner
    {
        private readonly IOptions<CertificateSettings> settings;

        public ClaimsSigner(
            IOptions<CertificateSettings> settings)
        {
            this.settings = settings;
        }

        #region Sign

        public string Sign(List<Claim> claims)
        {
            var algorithm = settings.Value.Algorithm;
            switch (algorithm.ToLower())
            {
                case "rsa":
                    return GetRSASignature(claims);
                case "ecdsa":
                    //throw new NotSupportedException("Disabled, because signature changes (by design) on each run and cannot be verified when hashed");
                    return GetECDSASignature(claims);
                default:
                    throw new Exception($"Invalid certificate signing algorithm {algorithm}");
            }
        }

        private string GetRSASignature(
            List<Claim> claims)
        {
            var jsonBytes = SerializeClaims(claims);
            var privateKey = settings.Value.SigningPrivateKey;
            if (string.IsNullOrEmpty(privateKey))
                throw new Exception("No private key available for signing profile");

            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);

            var signatureBytes = rsa.SignData(jsonBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var signatureHash = HashSignature(signatureBytes);

            return Convert.ToBase64String(signatureHash);
        }

        private string GetECDSASignature(
            List<Claim> claims)
        {
            var jsonBytes = SerializeClaims(claims);
            using var ecDsa = InitializeECDsa();

            var signatureBytes = ecDsa.SignData(jsonBytes, HashAlgorithmName.SHA256);

            return Convert.ToBase64String(signatureBytes);
        }

        private byte[] HashSignature(
            byte[] signatureBytes)
        {
            return SHA256.HashData(signatureBytes);
        }

        #endregion

        #region Verify

        public bool Verify(
            List<Claim> claims,
            string claimedSignature)
        {
            switch (settings.Value.Algorithm.ToLower())
            {
                case "rsa":
                {
                    var actualSignature = Sign(claims);
                    return claimedSignature == actualSignature;
                }
                case "ecdsa":
                    return VerifyECDSA(claims, claimedSignature);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool VerifyECDSA(
            List<Claim> claims,
            string claimedSignature)
        {
            using var ecDsa = InitializeECDsa();
            var jsonBytes = SerializeClaims(claims);
            return ecDsa.VerifyData(jsonBytes, Convert.FromBase64String(claimedSignature), HashAlgorithmName.SHA256);
        }

        #endregion

        private static byte[] SerializeClaims(
            List<Claim> claims)
        {
            var json = JsonConvert.SerializeObject(claims, Formatting.None);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            return jsonBytes;
        }

        private ECDsa InitializeECDsa()
        {
            var privateKey = settings.Value.SigningPrivateKey;
            if (string.IsNullOrEmpty(privateKey))
                throw new Exception("No private key available for signing profile");

            var ecDsa = ECDsa.Create();
            ecDsa.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);
            return ecDsa;
        }
    }
}
