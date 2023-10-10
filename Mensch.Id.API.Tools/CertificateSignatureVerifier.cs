using System;
using System.IO;
using Mensch.Id.API.Models;
using Mensch.Id.API.Workflow;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Mensch.Id.API.Tools
{
    internal class CertificateSignatureVerifier
    {
        private static readonly string PrivateKey = File.ReadAllText(@"C:\Temp\menschid-sign-key-rsa");

        [Test]
        public void Verify()
        {
            // ### START EDIT ####

            const string MenschId = "20230104-YMNKJ";
            const string OwnershipSecret = "JV4374 WY245C MXP55G UDAXMT 6HGLVP";
            const string Timestamp = "2023-02-23T12:49:05.080Z";
            const string Signature = "81h+qWZBi6kdMnueMIS0wrGPSMofdU8kMjBdsu7Zh0I=";
            const string Algorithm = "RSA";

            // ### END EDIT ####

            var profile = new AssignerControlledProfile(MenschId, null, DateTime.UtcNow, OwnershipSecret.Replace(" ",""));
            var settings = new OptionsWrapper<CertificateSettings>(
                new CertificateSettings
                {
                    Algorithm = Algorithm,
                    SigningPrivateKey = PrivateKey
                });
            var claimsSigner = new ClaimsSigner(settings);
            var certificateTimestamp = DateTime.Parse(Timestamp);
            var claims = SignedProfileBuilder.BuildClaims(profile, certificateTimestamp);

            Assert.That(claimsSigner.Verify(claims, Signature));
        }
    }
}
