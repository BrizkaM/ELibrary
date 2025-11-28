using ELibrary.Shared.Entities;
using ELibrary.Shared.Enums;

namespace ELibrary.Shared.Interfaces
{
    public interface IBookService
    {
        /// <summary>
        /// Creates book asynchronously.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <returns>Created book.</returns>
        Task<Book> CreateBookAsync(Book book);

        /// <summary>
        /// Borrows book for customer asynchronously. Decrease the ActualQuantity by 1.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="customerName">The customer's name.</param>
        /// <returns>Operation result and Updated book in case of success.</returns>
        Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> BorrowBookAsync(Guid bookId, string customerName);

        /// <summary>
        /// Returns book from customer asynchronously. Increases the ActualQuantity by 1.
        /// </summary>
        /// <param name="bookId">The book identifier.</param>
        /// <param name="customerName">The customer's name.</param>
        /// <returns>Operation result and Updated book in case of success.</returns>
        Task<(CustomerBookOperationResult OperationResult, Book? UpdatedBook)> ReturnBookAsync(Guid bookId, string customerName);
    }
}
