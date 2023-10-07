namespace Mensch.Id.API.Models.AccessControl;

public class LocalAnonymousAccount : PersonAccount, IAccountWithPassword
{
    public override AccountType AccountType => AccountType.LocalAnonymous;
    public string PasswordHash { get; set; }
    public string PasswordResetToken { get; set; }
}