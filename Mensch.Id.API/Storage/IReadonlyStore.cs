using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Storage
{
    public interface IReadonlyStore<T> where T: IId
    {
        IAsyncEnumerable<T> GetAllAsync();
        Task<bool> ExistsAsync(string id);
        Task<T> GetByIdAsync(string id);
        IAsyncEnumerable<T> SearchAsync(
            Expression<Func<T, bool>> filter,
            int? count = null,
            int? skip = null,
            Expression<Func<T, object>> orderBy = null,
            OrderDirection orderDirection = OrderDirection.Ascending);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter);
    }
}