using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Mensch.Id.API.Storage
{
    public class FilesStore : IFilesStore
    {
        private readonly string directory;

        public FilesStore(IOptions<FileStoreSettings> options)
        {
            directory = options.Value.Directory;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public Stream GetById(string documentId)
        {
            var filePath = GetFilePath(documentId);
            return File.OpenRead(filePath);
        }

        public async Task<long> GetFileSize(
            string documentId)
        {
            var filePath = GetFilePath(documentId);
            return new FileInfo(filePath).Length;
        }

        public async Task StoreAsync(string documentId, Stream stream)
        {
            var filePath = GetFilePath(documentId);
            await using var fileStream = File.OpenWrite(filePath);
            await stream.CopyToAsync(fileStream);
        }

        public void Delete(string documentId)
        {
            var filePath = GetFilePath(documentId);
            File.Delete(filePath);
        }

        private string GetFilePath(string documentId)
        {
            if (!Regex.IsMatch(documentId, "^[a-zA-Z0-9-_]+$"))
                throw new SecurityException("The provided document-ID is in an invalid format and may be vulnerable to path traversal");
            return Path.Combine(directory, documentId);
        }
    }
}
