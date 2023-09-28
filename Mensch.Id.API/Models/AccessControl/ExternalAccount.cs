namespace Mensch.Id.API.Models.AccessControl;

public class ExternalAccount : Account
{
    public override AccountType AccountType => AccountType.External;
    public LoginProvider LoginProvider { get; set; }
    public string ExternalId { get; set; }
}