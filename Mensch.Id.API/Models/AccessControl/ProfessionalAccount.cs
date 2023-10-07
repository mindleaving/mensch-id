namespace Mensch.Id.API.Models.AccessControl
{
    public abstract class ProfessionalAccount : Account, IAccountWithPassword
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
