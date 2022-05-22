using System.Collections.Generic;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow
{
    public static class PasswordResetEmailContent
    {
        public static readonly Dictionary<Language,string> Subject = new()
        {
            { Language.en, "Password reset for mensch.id account" }
        };

        public static readonly Dictionary<Language, string> Message = new()
        {
            { Language.en, "To reset your password click the link below or copy it into your browser.\r\n{link}" }
        };
    }
}
