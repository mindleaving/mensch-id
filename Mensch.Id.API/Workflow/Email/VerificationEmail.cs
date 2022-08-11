using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow.Email;

public class VerificationEmail
{
    public string RecipientAddress { get; set; }
    public string AccountId { get; set; }
    public string VerificationToken { get; set; }
    public Language PreferedLanguage { get; set; }
}