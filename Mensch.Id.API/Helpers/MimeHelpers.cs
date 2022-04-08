namespace Mensch.Id.API.Helpers
{
    public static class MimeHelpers
    {
        public static string GetContentTypeFromFileExtension(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".txt":
                    return "text/plain";
                case ".csv":
                    return "text/csv";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".zip":
                    return "application/zip";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
