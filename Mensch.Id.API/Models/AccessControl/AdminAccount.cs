namespace Mensch.Id.API.Models.AccessControl;

public class AdminAccount : LocalAccount
{
    public override AccountType AccountType => AccountType.Admin;


}