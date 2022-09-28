namespace Mensch.Id.API.Models;

public class LocalAccount : LocalAnonymousAccount
{
    public override AccountType AccountType => AccountType.Local;
    public string Email { get; set; }
    public string EmailVerificationAndPasswordResetSalt { get; set; }
    public string EmailVerificationToken { get; set; }
    public bool IsEmailVerified { get; set; }
}