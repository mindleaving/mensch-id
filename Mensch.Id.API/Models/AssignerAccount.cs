namespace Mensch.Id.API.Models
{
    public class AssignerAccount : LocalAccount
    {
        public override AccountType AccountType => AccountType.Assigner;
        public string Name { get; set; }
    }
}
