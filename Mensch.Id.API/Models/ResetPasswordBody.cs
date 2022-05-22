namespace Mensch.Id.API.Models
{
    public class ResetPasswordBody
    {
        public string AccountId { get; set; }
        public string ResetToken { get; set; }
        public string Password { get; set; }
    }
}