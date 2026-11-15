using Mensch.Id.API.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mensch.Id.API.Storage;

public class LoginProviderBsonSerializer : SerializerBase<LoginProvider>
{
    public override LoginProvider Deserialize(
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
        return LoginProvider.IsValid(value) ? new LoginProvider(value) : default;
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        LoginProvider value)
    {
        if (value.Value == null)
        {
            context.Writer.WriteNull();
            return;
        }
        context.Writer.WriteString(value.Value);
    }
}
