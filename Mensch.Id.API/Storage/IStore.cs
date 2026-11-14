using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Storage;

public interface IStore<T, in TKey> : IReadonlyStore<T, TKey> where T : IId<TKey>
{
    Task<StorageOperation> StoreAsync(T item);
    Task DeleteAsync(TKey id);
}

public interface IStore<T> : IStore<T, string>, IReadonlyStore<T> where T : IId
{
}
