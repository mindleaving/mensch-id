using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;

namespace Mensch.Id.API.Test.TestInfrastructure;

public class InMemoryStore<T> : IStore<T> where T : IId
{
    protected readonly ConcurrentDictionary<string, T> items = new();

    public IReadOnlyCollection<T> Items => items.Values.ToList();

    public async IAsyncEnumerable<T> GetAllAsync()
    {
        foreach (var item in items.Values.ToList())
        {
            yield return item;
        }
        await Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id)
    {
        return Task.FromResult(items.ContainsKey(id));
    }

    public Task<T> GetByIdAsync(string id)
    {
        if (id == null)
            return Task.FromResult(default(T));
        items.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public async IAsyncEnumerable<T> SearchAsync(
        Expression<Func<T, bool>> filter,
        int? count = null,
        int? skip = null,
        Expression<Func<T, object>> orderBy = null,
        OrderDirection orderDirection = OrderDirection.Ascending)
    {
        var compiledFilter = filter.Compile();
        IEnumerable<T> matches = items.Values.Where(compiledFilter);
        if (orderBy != null)
        {
            var compiledOrderBy = orderBy.Compile();
            matches = orderDirection == OrderDirection.Ascending
                ? matches.OrderBy(compiledOrderBy)
                : matches.OrderByDescending(compiledOrderBy);
        }
        if (skip.HasValue)
            matches = matches.Skip(skip.Value);
        if (count.HasValue)
            matches = matches.Take(count.Value);
        foreach (var match in matches.ToList())
        {
            yield return match;
        }
        await Task.CompletedTask;
    }

    public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter)
    {
        var compiledFilter = filter.Compile();
        var match = items.Values.FirstOrDefault(compiledFilter);
        return Task.FromResult(match);
    }

    public Task<StorageOperation> StoreAsync(T item)
    {
        var isNew = !items.ContainsKey(item.Id);
        items[item.Id] = item;
        return Task.FromResult(isNew ? StorageOperation.Created : StorageOperation.Changed);
    }

    public Task DeleteAsync(string id)
    {
        items.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
