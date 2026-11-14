using System.Collections.Generic;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow.Email;

public static class ExternalServiceVerificationEmailContent
{
    public const string VerificationLinkPlaceholder = "{link}";
    public const string ServiceNamePlaceholder = "{serviceName}";

    public static readonly Dictionary<Language, string> Subject = new()
    {
        { Language.en, "Verify the e-mail address of your service at mensch.ID" }
    };

    public static readonly Dictionary<Language, string> Message = new()
    {
        {
            Language.en,
            $"The service '{ServiceNamePlaceholder}' was registered at mensch.ID with this e-mail address.\r\n"
            + "To verify the address, click the link below or copy it into your browser. "
            + "After verification, an administrator can approve the service.\r\n"
            + VerificationLinkPlaceholder
        }
    };
}
