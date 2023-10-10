namespace Mensch.Id.API.Models.AccessControl;

public class ExternalAccount : PersonAccount
{
    public override AccountType AccountType => AccountType.External;
    public LoginProvider LoginProvider { get; set; }
    public string ExternalId { get; set; }
}