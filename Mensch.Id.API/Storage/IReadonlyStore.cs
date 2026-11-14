using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Storage;

public interface IReadonlyStore<T, in TKey> where T : IId<TKey>
{
    IAsyncEnumerable<T> GetAllAsync();
    Task<bool> ExistsAsync(TKey id);
    Task<T> GetByIdAsync(TKey id);
    IAsyncEnumerable<T> SearchAsync(
        Expression<Func<T, bool>> filter,
        int? count = null,
        int? skip = null,
        Expression<Func<T, object>> orderBy = null,
        OrderDirection orderDirection = OrderDirection.Ascending);
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter);
}

public interface IReadonlyStore<T> : IReadonlyStore<T, string> where T : IId
{
}
