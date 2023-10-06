namespace Mensch.Id.API.Models.AccessControl;

public class AssignerAccount : ProfessionalAccount
{
    public override AccountType AccountType => AccountType.Assigner;

    public string Name { get; set; }
    public string LogoId { get; set; }
    public string LogoImageType { get; set; }

    public Contact Contact { get; set; }
}