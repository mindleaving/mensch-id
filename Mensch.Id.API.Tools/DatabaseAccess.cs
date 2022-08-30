using MongoDB.Driver;

namespace Mensch.Id.API.Tools
{
    public abstract class DatabaseAccess
    {
        private const string DatabaseName = "MenschID";
        private readonly IMongoClient client;
        private readonly IMongoDatabase database;

        protected DatabaseAccess()
        {
            client = new MongoClient();
            database = client.GetDatabase(DatabaseName);
        }

        protected IMongoCollection<T> GetCollection<T>(string collectionName = null) => database.GetCollection<T>(collectionName ?? typeof(T).Name);

    }
}
