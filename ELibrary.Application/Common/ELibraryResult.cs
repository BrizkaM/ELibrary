namespace ELibrary.Application.Common
{
    /// <summary>
    /// Represents the result of an operation that can either succeed or fail.
    /// Provides explicit success/failure handling instead of relying on exceptions.
    /// </summary>
    /// <typeparam name="T">The type of the value returned on success</typeparam>
    public class ELibraryResult<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the value returned by the operation (only available on success).
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Gets the error message (only available on failure).
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Gets the error code for categorizing failures (optional).
        /// </summary>
        public string? ErrorCode { get; }

        /// <summary>
        /// Private constructor to enforce factory methods.
        /// </summary>
        private ELibraryResult(bool isSuccess, T? value, string? error, string? errorCode)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a successful result with a value.
        /// </summary>
        /// <param name="value">The value to return</param>
        /// <returns>A successful Result</returns>
        public static ELibraryResult<T> Success(T value)
        {
            return new ELibraryResult<T>(true, value, null, null);
        }

        /// <summary>
        /// Creates a failed result with an error message.
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>A failed Result</returns>
        public static ELibraryResult<T> Failure(string error)
        {
            return new ELibraryResult<T>(false, default, error, null);
        }

        /// <summary>
        /// Creates a failed result with an error message and error code.
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="errorCode">The error code (e.g., "NOT_FOUND", "OUT_OF_STOCK")</param>
        /// <returns>A failed Result</returns>
        public static ELibraryResult<T> Failure(string error, string errorCode)
        {
            return new ELibraryResult<T>(false, default, error, errorCode);
        }

        /// <summary>
        /// Matches the result to either success or failure handler.
        /// </summary>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <param name="onSuccess">Handler for successful result</param>
        /// <param name="onFailure">Handler for failed result</param>
        /// <returns>The result of the appropriate handler</returns>
        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<string, string?, TResult> onFailure)
        {
            return IsSuccess
                ? onSuccess(Value!)
                : onFailure(Error!, ErrorCode);
        }

        /// <summary>
        /// Maps the value of a successful result to a new type.
        /// </summary>
        /// <typeparam name="TNew">The new type</typeparam>
        /// <param name="mapper">Function to map the value</param>
        /// <returns>A new Result with the mapped value</returns>
        public ELibraryResult<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            return IsSuccess
                ? ELibraryResult<TNew>.Success(mapper(Value!))
                : ELibraryResult<TNew>.Failure(Error!, ErrorCode ?? string.Empty);
        }

        /// <summary>
        /// Binds the result to another operation that returns a Result.
        /// </summary>
        /// <typeparam name="TNew">The new type</typeparam>
        /// <param name="binder">Function that returns a new Result</param>
        /// <returns>The result of the binder function, or the original failure</returns>
        public ELibraryResult<TNew> Bind<TNew>(Func<T, ELibraryResult<TNew>> binder)
        {
            return IsSuccess
                ? binder(Value!)
                : ELibraryResult<TNew>.Failure(Error!, ErrorCode ?? string.Empty);
        }

        /// <summary>
        /// Implicitly converts a value to a successful Result.
        /// </summary>
        public static implicit operator ELibraryResult<T>(T value) => Success(value);

        /// <summary>
        /// Returns a string representation of the Result.
        /// </summary>
        public override string ToString()
        {
            return IsSuccess
                ? $"Success: {Value}"
                : $"Failure: {Error} (Code: {ErrorCode ?? "N/A"})";
        }
    }

    /// <summary>
    /// Non-generic Result for operations that don't return a value.
    /// </summary>
    public class ELibraryResult
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the error message (only available on failure).
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Gets the error code for categorizing failures (optional).
        /// </summary>
        public string? ErrorCode { get; }

        /// <summary>
        /// Private constructor to enforce factory methods.
        /// </summary>
        private ELibraryResult(bool isSuccess, string? error, string? errorCode)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static ELibraryResult Success()
        {
            return new ELibraryResult(true, null, null);
        }

        /// <summary>
        /// Creates a failed result with an error message.
        /// </summary>
        /// <param name="error">The error message</param>
        public static ELibraryResult Failure(string error)
        {
            return new ELibraryResult(false, error, null);
        }

        /// <summary>
        /// Creates a failed result with an error message and error code.
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="errorCode">The error code</param>
        public static ELibraryResult Failure(string error, string errorCode)
        {
            return new ELibraryResult(false, error, errorCode);
        }

        /// <summary>
        /// Matches the result to either success or failure handler.
        /// </summary>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <param name="onSuccess">Handler for successful result</param>
        /// <param name="onFailure">Handler for failed result</param>
        public TResult Match<TResult>(
            Func<TResult> onSuccess,
            Func<string, string?, TResult> onFailure)
        {
            return IsSuccess
                ? onSuccess()
                : onFailure(Error!, ErrorCode);
        }

        /// <summary>
        /// Returns a string representation of the Result.
        /// </summary>
        public override string ToString()
        {
            return IsSuccess
                ? "Success"
                : $"Failure: {Error} (Code: {ErrorCode ?? "N/A"})";
        }
    }

    /// <summary>
    /// Common error codes used throughout the application.
    /// </summary>
    public static class ErrorCodes
    {
        public const string NotFound = "NOT_FOUND";
        public const string OutOfStock = "OUT_OF_STOCK";
        public const string Conflict = "CONFLICT";
        public const string ValidationError = "VALIDATION_ERROR";
        public const string DuplicateIsbn = "DUPLICATE_ISBN";
        public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
        public const string InvalidOperation = "INVALID_OPERATION";
    }
}