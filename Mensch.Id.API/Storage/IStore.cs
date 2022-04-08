using System.Threading.Tasks;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Storage
{
    public interface IStore<T> : IReadonlyStore<T> where T : IId
    {
        Task<StorageOperation> StoreAsync(T item);
        Task DeleteAsync(string id);
    }
}
