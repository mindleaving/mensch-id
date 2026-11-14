using System;
using Mensch.Id.API.Helpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mensch.Id.API.Storage;

public class ValidatedStringValueBsonSerializer<T> : SerializerBase<T> where T : struct, IValidatedStringValue<T>
{
    public override T Deserialize(
        BsonDeserializationContext context,
        BsonDeserializationArgs args)
    {
        var currentBsonType = context.Reader.GetCurrentBsonType();
        if (currentBsonType != BsonType.String)
        {
            context.Reader.SkipValue();
            return default;
        }
        var value = context.Reader.ReadString();
        try
        {
            return T.Parse(value);
        }
        catch (ArgumentException)
        {
            return default;
        }
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        T value)
    {
        if (value.Value == null)
        {
            context.Writer.WriteNull();
            return;
        }
        context.Writer.WriteString(value.Value);
    }
}
