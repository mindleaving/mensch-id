using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using MongoDB.Driver;

namespace Mensch.Id.API.Storage;

public class GenericStore<T, TKey> : GenericReadonlyStore<T, TKey>, IStore<T, TKey> where T : IId<TKey>
{
    public GenericStore(
        IMongoDatabase mongoDatabase,
        string collectionName = null)
        : base(mongoDatabase, collectionName)
    {
    }

    public async Task<StorageOperation> StoreAsync(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        var filter = BuildIdFilter(item.Id);
        var options = new ReplaceOptions { IsUpsert = true };
        var result = await collection.ReplaceOneAsync(filter, item, options);
        return result.MatchedCount == 1 ? StorageOperation.Changed : StorageOperation.Created;
    }

    public Task DeleteAsync(TKey id)
    {
        var filter = BuildIdFilter(id);
        return collection.DeleteOneAsync(filter);
    }
}

public class GenericStore<T> : GenericStore<T, string>, IStore<T> where T : IId
{
    public GenericStore(
        IMongoDatabase mongoDatabase,
        string collectionName = null)
        : base(mongoDatabase, collectionName)
    {
    }
}
