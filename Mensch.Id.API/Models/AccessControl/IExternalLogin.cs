namespace Mensch.Id.API.Models.AccessControl
{
    public interface IExternalLogin
    {
        public LoginProvider LoginProvider { get; set; }
        public string ExternalId { get; set; }
    }
}