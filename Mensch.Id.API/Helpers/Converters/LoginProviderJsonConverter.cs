using System;
using Newtonsoft.Json;

using Mensch.Id.API.Models;

namespace Mensch.Id.API.Helpers.Converters;

public class LoginProviderJsonConverter : JsonConverter<LoginProvider>
{
    public override void WriteJson(
        JsonWriter writer,
        LoginProvider value,
        JsonSerializer serializer)
    {
        writer.WriteValue(value.Value);
    }

    public override LoginProvider ReadJson(
        JsonReader reader,
        Type objectType,
        LoginProvider existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String)
            throw new JsonSerializationException($"Expected string for login provider, but got {reader.TokenType}");
        var value = (string)reader.Value;
        return new LoginProvider(value);
    }
}
