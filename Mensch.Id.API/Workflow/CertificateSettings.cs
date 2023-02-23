namespace Mensch.Id.API.Workflow;

public class CertificateSettings
{
    public const string AppSettingsSectionName = "Certificate";

    public string Algorithm { get; set; }
    public string SigningPrivateKey { get; set; }
}