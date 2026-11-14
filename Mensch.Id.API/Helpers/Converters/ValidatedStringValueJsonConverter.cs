using System;
using Newtonsoft.Json;

namespace Mensch.Id.API.Helpers.Converters;

public class ValidatedStringValueJsonConverter<T> : JsonConverter where T : struct, IValidatedStringValue<T>
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(T) || objectType == typeof(T?);
    }

    public override void WriteJson(
        JsonWriter writer,
        object value,
        JsonSerializer serializer)
    {
        if (value is not T typedValue || typedValue.Value == null)
        {
            writer.WriteNull();
            return;
        }
        writer.WriteValue(typedValue.Value);
    }

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            if (objectType == typeof(T?))
                return null;
            throw new JsonSerializationException($"{typeof(T).Name} must not be null");
        }
        if (reader.TokenType != JsonToken.String)
            throw new JsonSerializationException($"Expected string for {typeof(T).Name}, but got {reader.TokenType}");
        var value = (string)reader.Value;
        try
        {
            return T.Parse(value);
        }
        catch (ArgumentException exception)
        {
            throw new JsonSerializationException(exception.Message, exception);
        }
    }
}
