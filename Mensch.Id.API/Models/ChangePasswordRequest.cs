namespace Mensch.Id.API.Models
{
    public class ChangePasswordRequest
    {
        public string AccountId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
