﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mensch.Id.API.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuthenticationErrorType
    {
        Unknown = 0,
        InvalidUserOrPassword = 2,
        AuthenticationMethodNotAvailable = 3,
        EmailNotVerified = 4
    }
}