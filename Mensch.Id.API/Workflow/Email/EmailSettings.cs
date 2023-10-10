namespace Mensch.Id.API.Workflow.Email;

public class EmailSettings
{
    public const string SettingsName = "Email";

    public bool UseDummySender { get; set; }

    public string SmtpServerName { get; set; }
    public ushort SmtpServerPort { get; set; }
    public bool UseAuthentication { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }

    public string FromAddress { get; set; }
    public string WebsiteBaseUrl { get; set; }
}