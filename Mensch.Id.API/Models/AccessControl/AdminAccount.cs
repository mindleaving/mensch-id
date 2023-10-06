namespace Mensch.Id.API.Models.AccessControl;

public class AdminAccount : ProfessionalAccount
{
    public override AccountType AccountType => AccountType.Admin;


}