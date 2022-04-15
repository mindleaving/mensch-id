namespace Mensch.Id.API.Models
{
    public interface IExternalLogin
    {
        public LoginProvider LoginProvider { get; set; }
        public string ExternalId { get; set; }
    }
}