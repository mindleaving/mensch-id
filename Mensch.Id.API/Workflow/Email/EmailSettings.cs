namespace Mensch.Id.API.Workflow.Email;

public class EmailSettings
{
    public const string SettingsName = "Email";

    public string SmtpServerName { get; set; }
    public ushort SmtpServerPort { get; set; }
    public string FromAddress { get; set; }
}