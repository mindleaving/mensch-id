namespace Mensch.Id.API.Models;

public class ExternalAccount : Account
{
    public override AccountType AccountType => AccountType.External;
    public LoginProvider LoginProvider { get; set; }
    public string ExternalId { get; set; }
}