using System.IO;
using System.Threading.Tasks;

namespace Mensch.Id.API.Storage
{
    public interface IFilesStore
    {
        Stream GetById(string documentId);

        Task<long> GetFileSize(
            string documentId);

        Task StoreAsync(
            string documentId,
            Stream stream);

        void Delete(string documentId);
    }
}
