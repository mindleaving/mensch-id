using System.IO;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mensch.Id.API.Tools
{
    public abstract class DatabaseAccess
    {
        private const string DatabaseName = "MenschID";
        private readonly IMongoDatabase database;

        protected DatabaseAccess()
        {
            var settings = new MongoClientSettings
            {
                Credential = MongoCredential.CreateCredential("admin", "dev", File.ReadAllText(@"C:\Temp\mongodb-dev-credential.txt"))
            };
            var mongoClient = new MongoClient(settings);
            ConventionRegistry.Register("EnumStringConvetion", new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String)
            }, type => true);
            database = mongoClient.GetDatabase(DatabaseName);
        }

        protected IMongoCollection<T> GetCollection<T>(string collectionName = null) => database.GetCollection<T>(collectionName ?? typeof(T).Name);

    }
}
