using ELibrary.Application.DTOs;
using ELibrary.Domain.Enums;

namespace ELibrary.Application.Interfaces
{
    public interface IBookService
    {
        /// <summary>
        /// Gets all books asynchronously.
        /// </summary>
        /// <returns>All books.</returns>
        Task<IEnumerable<BookDto>> GetAllBooksAsync();

        /// <summary>
        /// Creates book asynchronously.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <returns>Created book.</returns>
        Task<BookDto> CreateBookAsync(BookDto book);

        /// <summary>
        /// Searches books asynchronously.
        /// </summary>
        /// <param name="title">Name of the Book.</param>
        /// <param name="author">Author of the book.</param>
        /// <param name="isbn">Isbn of the book.</param>
        /// <returns></returns>
        Task<IEnumerable<BookDto>> SearchBooksAsync(string? title, string? author, string? isbn);

        /// <summary>
        /// Borrows book for customer asynchronously. Decrease the ActualQuantity by 1.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="customerName">The customer's name.</param>
        /// <returns>Operation result and Updated book in case of success.</returns>
        Task<(CustomerBookOperationResult OperationResult, BookDto? UpdatedBook)> BorrowBookAsync(Guid bookId, string customerName);

        /// <summary>
        /// Returns book from customer asynchronously. Increases the ActualQuantity by 1.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="customerName">The customer's name.</param>
        /// <returns>Operation result and Updated book in case of success.</returns>
        Task<(CustomerBookOperationResult OperationResult, BookDto? UpdatedBook)> ReturnBookAsync(Guid bookId, string customerName);
    }
}
