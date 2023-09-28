namespace Mensch.Id.API.Models.AccessControl;

public class AssignerAccount : LocalAccount
{
    public override AccountType AccountType => AccountType.Assigner;

    public string Name { get; set; }
    public string LogoUrl { get; set; }

    public Contact Contact { get; set; }
}