using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow.Email;

public class PasswordResetEmail
{
    public string RecipientAddress { get; set; }
    public string AccountId { get; set; }
    public string ResetToken { get; set; }
    public Language PreferedLanguage { get; set; }
}