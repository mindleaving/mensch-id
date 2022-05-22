﻿using System.Collections.Generic;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow
{
    public static class VerificationEmailContent
    {
        public static readonly Dictionary<Language,string> Subject = new()
        {
            { Language.en, "Verify your mensch.id account"}
        };

        public static readonly Dictionary<Language, string> Message = new()
        {
            { Language.en, "To verify your account click the link below or copy it into your browser.\r\n{link}" }
        };
    }
}
