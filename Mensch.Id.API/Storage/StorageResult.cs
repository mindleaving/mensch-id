namespace Mensch.Id.API.Storage
{
    public class StorageResult
    {
        private StorageResult(
            bool isSuccess,
            StoreErrorType? errorType)
        {
            IsSuccess = isSuccess;
            ErrorType = errorType;
        }

        public static StorageResult Success()
        {
            return new StorageResult(true, null);
        }

        public static StorageResult Error(StoreErrorType errorType)
        {
            return new StorageResult(false, errorType);
        }

        public bool IsSuccess { get; }
        public StoreErrorType? ErrorType { get; }
    }
}