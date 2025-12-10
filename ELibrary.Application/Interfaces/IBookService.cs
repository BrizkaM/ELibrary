using ELibrary.Application.Common;
using ELibrary.Application.DTOs;

namespace ELibrary.Application.Interfaces
{
    public interface IBookService
    {
        /// <summary>
        /// Gets all books asynchronously.
        /// </summary>
        /// <returns>Result containing all books.</returns>
        Task<ELibraryResult<IEnumerable<BookDto>>> GetAllBooksAsync();

        /// <summary>
        /// Creates book asynchronously.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <returns>Result containing created book or error.</returns>
        Task<ELibraryResult<BookDto>> CreateBookAsync(BookDto book);

        /// <summary>
        /// Searches books asynchronously.
        /// </summary>
        /// <param name="title">Name of the Book.</param>
        /// <param name="author">Author of the book.</param>
        /// <param name="isbn">Isbn of the book.</param>
        /// <returns>Result containing matching books or error.</returns>
        Task<ELibraryResult<IEnumerable<BookDto>>> SearchBooksAsync(string? title, string? author, string? isbn);

        /// <summary>
        /// Borrows book for customer asynchronously. Decrease the ActualQuantity by 1.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="customerName">The customer's name.</param>
        /// <returns>Result containing updated book or error (NotFound, OutOfStock, Conflict).</returns>
        Task<ELibraryResult<BookDto>> BorrowBookAsync(Guid bookId, string customerName);

        /// <summary>
        /// Returns book from customer asynchronously. Increases the ActualQuantity by 1.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="customerName">The customer's name.</param>
        /// <returns>Result containing updated book or error (NotFound, Conflict).</returns>
        Task<ELibraryResult<BookDto>> ReturnBookAsync(Guid bookId, string customerName);
    }
}