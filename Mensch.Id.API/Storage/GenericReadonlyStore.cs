using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using MongoDB.Driver;

namespace Mensch.Id.API.Storage;

public class GenericReadonlyStore<T, TKey> : IReadonlyStore<T, TKey> where T : IId<TKey>
{
    protected readonly IMongoDatabase database;
    protected readonly IMongoCollection<T> collection;

    public GenericReadonlyStore(
        IMongoDatabase mongoDatabase,
        string collectionName = null)
    {
        database = mongoDatabase;
        collection = mongoDatabase.GetCollection<T>(collectionName ?? typeof(T).Name);
    }

    public IAsyncEnumerable<T> GetAllAsync()
    {
        return collection.Find(FilterDefinition<T>.Empty).ToAsyncEnumerable();
    }

    public IAsyncEnumerable<T> SearchAsync(
        Expression<Func<T, bool>> filter,
        int? count = null,
        int? skip = null,
        Expression<Func<T, object>> orderBy = null,
        OrderDirection orderDirection = OrderDirection.Ascending)
    {
        var findExpression = collection.Find(filter ?? (x => true));
        if (orderBy != null)
        {
            findExpression = orderDirection == OrderDirection.Ascending
                ? findExpression.SortBy(orderBy)
                : findExpression.SortByDescending(orderBy);
        }
        return findExpression.Skip(skip).Limit(count).ToAsyncEnumerable();
    }

    public Task<bool> ExistsAsync(TKey id)
    {
        var filter = BuildIdFilter(id);
        return collection.Find(filter).AnyAsync();
    }

    public Task<T> GetByIdAsync(TKey id)
    {
        var filter = BuildIdFilter(id);
        return collection.Find(filter).FirstOrDefaultAsync();
    }

    public Task<T> FirstOrDefaultAsync(
        Expression<Func<T, bool>> filter)
    {
        return collection.Find(filter).FirstOrDefaultAsync();
    }

    protected static FilterDefinition<T> BuildIdFilter(TKey id)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        return Builders<T>.Filter.Eq(x => x.Id, id);
    }
}

public class GenericReadonlyStore<T> : GenericReadonlyStore<T, string>, IReadonlyStore<T> where T : IId
{
    public GenericReadonlyStore(
        IMongoDatabase mongoDatabase,
        string collectionName = null)
        : base(mongoDatabase, collectionName)
    {
    }
}
