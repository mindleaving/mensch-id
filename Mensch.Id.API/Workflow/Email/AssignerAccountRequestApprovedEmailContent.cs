using Mensch.Id.API.Models;
using System.Collections.Generic;

namespace Mensch.Id.API.Workflow.Email
{
    public static class AssignerAccountRequestApprovedEmailContent
    {
        public const string NamePlaceholder = "{name}";

        public static readonly Dictionary<Language,string> Subject = new()
        {
            { Language.en, "Your mensch.ID assigner account has been approved" }
        };

        public static readonly Dictionary<Language, string> Message = new()
        {
            { Language.en, $"Dear {NamePlaceholder}.\r\n\r\n"
                           + $"Your assigner account has been approved. You can now assign mensch.IDs for other people. Please remember to provide them with their ownership certificates.\r\n\r\n"
                           + $"To set your password click the link below or copy it into your browser.\r\n{PasswordResetEmailContent.ResetLinkPlaceholder}" }
        };
    }
}
