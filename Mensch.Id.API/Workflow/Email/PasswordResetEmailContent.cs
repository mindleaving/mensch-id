using System.Collections.Generic;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow.Email
{
    public static class PasswordResetEmailContent
    {
        public const string ResetLinkPlaceholder = "{link}";

        public static readonly Dictionary<Language,string> Subject = new()
        {
            { Language.en, "Password reset for mensch.id account" }
        };

        public static readonly Dictionary<Language, string> Message = new()
        {
            { Language.en, $"To reset your password click the link below or copy it into your browser.\r\n{ResetLinkPlaceholder}" }
        };
    }
}
