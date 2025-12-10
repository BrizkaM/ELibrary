using ELibrary.Application.Commands.Books;
using ELibrary.Application.Common;
using ELibrary.Application.DTOs;
using ELibrary.Application.Queries.Books;

namespace ELibrary.Application.Interfaces
{
    /// <summary>
    /// Book service interface using CQRS pattern (Commands and Queries).
    /// Commands modify state, Queries only read data.
    /// </summary>
    public interface IBookService
    {
        // ============================================================
        // QUERIES (Read Operations - No Side Effects)
        // ============================================================

        /// <summary>
        /// Handles query to get all books from the library.
        /// </summary>
        /// <param name="query">The get all books query</param>
        /// <returns>Result containing collection of all books</returns>
        Task<ELibraryResult<IEnumerable<BookDto>>> HandleAsync(GetAllBooksQuery query);

        /// <summary>
        /// Handles query to search books by criteria.
        /// </summary>
        /// <param name="query">The search books query with criteria</param>
        /// <returns>Result containing collection of matching books</returns>
        Task<ELibraryResult<IEnumerable<BookDto>>> HandleAsync(SearchBooksQuery query);

        // ============================================================
        // COMMANDS (Write Operations - Modify State)
        // ============================================================

        /// <summary>
        /// Handles command to create a new book in the library.
        /// </summary>
        /// <param name="command">The create book command</param>
        /// <returns>Result containing the created book or error</returns>
        Task<ELibraryResult<BookDto>> HandleAsync(CreateBookCommand command);

        /// <summary>
        /// Handles command to borrow a book from the library.
        /// Decreases book quantity by 1 and records the transaction.
        /// </summary>
        /// <param name="command">The borrow book command</param>
        /// <returns>Result containing updated book or error (NotFound, OutOfStock, Conflict)</returns>
        Task<ELibraryResult<BookDto>> HandleAsync(BorrowBookCommand command);

        /// <summary>
        /// Handles command to return a borrowed book to the library.
        /// Increases book quantity by 1 and records the transaction.
        /// </summary>
        /// <param name="command">The return book command</param>
        /// <returns>Result containing updated book or error (NotFound, Conflict)</returns>
        Task<ELibraryResult<BookDto>> HandleAsync(ReturnBookCommand command);
    }
}