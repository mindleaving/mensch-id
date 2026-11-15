using System;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow.Email;

public class ExternalServiceVerificationEmail
{
    public string RecipientAddress { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; }
    public string VerificationToken { get; set; }
    public Language PreferedLanguage { get; set; }
}
