namespace ELibrary.Domain.Enums
{
    /// <summary>
    /// Customer book operation results for Borrowing and Returning books.
    /// </summary>
    public enum CustomerBookOperationResult
    {
        /// <summary>
        /// Result indicates a successful operation.
        /// </summary>
        Success,

        /// <summary>
        /// Result indicates that the book is out of stock.
        /// </summary>
        OutOfStock,

        /// <summary>
        /// Result indicates that the book was not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// Result indicates a concurrency conflict occurred.
        /// </summary>
        Conflict
    }
}
